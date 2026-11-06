# Arranque: taller de batería v1

Sesión desde la raíz `D:\Projects`. Proyecto principal: ALWTTT (fuente en `ALWTTT25/Docs/reference/taller-bateria/`); se publica en la web privada.

## Contexto
Primera de una serie de páginas por tipo de pista (batería → bajo → melodía y voz con Pink Trombone) para **explorar, evaluar y, después, crear** patrones con los assets reales del proyecto y exportarlos de vuelta. Modelo a seguir: el taller armónico (`ALWTTT25/Docs/reference/taller-armonia/`: `template.html` más `build.py`, que lee los assets y genera la página; `--web` la copia a `web/claudiobustamante-site/public/privado/taller-armonia/`). Léelo antes de diseñar.

Lo que existe para batería (solo lectura; no se editan el paquete MidiGenPlay ni los assets):
- Editor y formato de texto: `MidiGenPlay/MidiGenPlay/Editor/DrumPatternEditorWindow.cs`, `DrumPatternEditorImporter.cs`, `DrumPatternCatalogueWizard.cs`, `DrumMidiImporter.cs`; generador con LLM en `DrumPatternLLMGenerator.cs` y `DrumPatternLLMPromptBuilder.cs`.
- Autoridad del formato: `MidiGenPlay/MidiGenPlay/Documentation~/authoring/SSoT_Authoring_Rhythm_Patterns.md`. El skill `rhythm-pattern-generator` describe el DSL (tarjeta y cadenas de glifos por carril).
- Assets: paletas en `ALWTTT25/Assets/Resources/ScriptableObjects/Drums/Palettes/` y los patrones que referencian (inventariar dónde viven: buscar los `.asset` de patrones de batería en ALWTTT y en MidiGenPlay).

## Objetivo v1 (explorar y evaluar; crear queda para la v2)
1. `build.py` que lee los assets (YAML de Unity) y produce un JSON de patrones y paletas: compás, subdivisiones, carriles (instrumento GM, velocidades), nombre, paleta y etiquetas.
2. Página: catálogo por paleta y compás, rejilla tipo step sequencer de solo lectura, reproducción con un kit sintetizado en WebAudio (bombo, caja, hi-hat cerrado y abierto, toms, platos; que suene decente es requisito), BPM, bucle y swing si el formato lo tiene.
3. Evaluar: ✓ ~ ✗ y notas por patrón (localStorage en la web; si también se publica como artefacto, `db`).
4. Exportar: botón «Copiar para el editor» que da el texto que acepta el Import de `DrumPatternEditorWindow`. Verificarlo contra el parser y el importer: cero avisos en los patrones del catálogo y prueba de ida y vuelta (asset → texto → parser).

## Decisiones a presentar al abrir (formato D)
- Alcance del inventario: solo los patrones que usan las paletas, o todos.
- Kit: síntesis pura, o muestras propias pequeñas con licencia limpia.
- Puerta estética: Claudio elige el aspecto entre 2 o 3 opciones antes de pulir.

## Reglas
- Dudas musicales (nombres, qué evaluar, qué es un buen groove por género) → subagente que actúa como Music Lab (memoria `consultar-music-lab`).
- Commits locales con pathspec en ALWTTT y en la web. Push solo con el OK de Claudio y con el método de worktree (no publicar trabajo ajeno).
- Hay sesiones en paralelo: el LLM en el taller armónico (toca `taller-armonia/` y `web/.../src/llm/`) y la encuesta pública. No toquéis los mismos archivos; el enlace en `src/pages/privado/index.astro`, al final y con pathspec.
- La demo del 11-dic manda: nada del juego ni del paquete.

## Hecho cuando
Página en `/privado/taller-bateria/` enlazada desde `/privado/`, todos los patrones del inventario suenan, la exportación está verificada contra el parser, Claudio la probó en el móvil, README de la carpeta, `foco.md` y `proyectos.md` al día y commits hechos. Dejar anotado qué haría falta para la v2 (crear y LLM, reutilizando lo del taller armónico).

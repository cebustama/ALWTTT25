# Arranque: LLM en el taller armónico

Sesión desde la raíz `D:\Projects`. Proyecto principal: ALWTTT (fuente del taller); se toca también la web.

## Contexto
El taller armónico (`ALWTTT25/Docs/reference/taller-armonia/`: `template.html`, `build.py`, `README.md`) carga progresiones en romanos, las toca y les aplica niveles 1–3 con reglas fijas (v3, función `arrange(base, level, nV, beats)`). Prueba de oído de la v3: 16/16 ✓ (`prueba-oido-v3.md`). Se publica en dos sitios: el artefacto https://claude.ai/artifact/6ABsXYisfqmwdof7PhsYik y la web privada https://claudiobustamante.com/privado/taller-armonia/ (`build.py --web`).

La web tiene un proxy LLM `/api/llm` con lista blanca de tareas: `web/claudiobustamante-site/functions/api/llm.js`, `src/llm/tareas/index.js` y `taller.js` (ejemplo de tarea), guía en `Documentation~/reference/llm-proxy.md`. Haiku por defecto; Sonnet con identidad de Access.

En Unity existe la ventana que crea progresiones con LLM: `MidiGenPlay/MidiGenPlay/Editor/ChordProgressionEditorWindow.LLM.cs`, `ChordProgressionLLMPromptBuilder.cs`, `ChordProgressionLLMResponseHandler.cs`, `ChordLLMFieldPlan.cs`, y `Documentation~/authoring/SSoT_Authoring_LLM_Generation.md`. Úsalos como referencia de vocabulario y reglas (solo lectura: no se edita el paquete).

## Objetivo
1. **Crear desde una frase:** un campo «Describe la progresión» → tarea nueva `armonia.generar` → el modelo devuelve texto en el formato que ya lee el taller (romanos, modo, compás). Validar con el lector del taller; si no parsea, un reintento con el error; si vuelve a fallar, mensaje claro.
2. **Modificar la cargada** («nivel 3 menos loco», «más acordes de paso en el 2»): el LLM **no reescribe acordes**; ajusta **perillas** de las reglas. Primero hay que sacar esas perillas de `arrange` a un objeto de parámetros visible en la interfaz (presupuesto de cambios por vuelta y nivel; tipos de cambio permitidos: dominantes secundarias, sustitutos tritonales, iv prestado, ♭VI, sus4, novenas, vuelta al inicio). Tarea `armonia.ajustar`: recibe la progresión, el nivel y las perillas actuales; devuelve perillas nuevas en JSON y una frase de por qué. Así sigue siendo determinista y se puede guardar con la progresión.

## Decisiones a presentar al abrir (formato D)
- Las perillas por defecto deben reproducir exactamente la v3: prueba de oro con las 8 progresiones de `prueba-oido-v3.md` y el resto del catálogo, salida idéntica antes y después.
- Modelo por defecto para estas tareas (Haiku o Sonnet), tras medir 10 frases de prueba.
- En el artefacto, `/api/llm` no es accesible: botones ocultos, o la capacidad `sample` del artefacto (cargar el skill `artifact-capabilities` antes).

## Reglas
- Dudas musicales → consultar a Music Lab con un subagente (memoria `consultar-music-lab`), sobre todo para nombrar y limitar las perillas.
- Notas de la prueba de oído para tener en cuenta (no obligatorias): el nivel 2 con solo cambio de cualidad se nota poco; el ♭V7 final de la Royal road se aleja.
- Commits locales con pathspec en ALWTTT y en la web (hay cambios de otras sesiones sin commitear). Push de la web solo con el OK de Claudio y con el método de worktree (no publicar trabajo ajeno).
- Hay sesiones en paralelo: la página de batería (`taller-bateria/`) y la encuesta pública. No toquéis los mismos archivos; si hay que tocar `src/pages/privado/index.astro`, al final y con pathspec.
- La demo del 11-dic manda: nada del juego ni del paquete.

## Hecho cuando
Las dos funciones van en `/privado/taller-armonia/`, la prueba de oro sale idéntica, 10 frases medidas (válidas y útiles), README del taller al día, `foco.md` actualizado y commits hechos.

# Prueba de escucha a ciegas (armonía de Sibi)

Página privada: https://claude.ai/artifact/9ZE34uZkFr8m7aS9WncPq6 (creada el 2026-11-02).

25 muestras: 4 progresiones por cada una de las seis paletas propuestas en el [wrapped de la armonía](../armonia-wrapped/), más la Axis repetida como control. Todas suenan con el mismo sintetizador (piano eléctrico FM y bajo, Web Audio), a 96 BPM, 64 tiempos, en un tono al azar. Quien escucha responde gusto, sorpresa, tensión (de 1 a 5) y qué promesa de carta le sugiere cada muestra. Al terminar ve qué era cada una.

- `export_listen.py`: saca los eventos de los assets reales (y de las candidatas) con el parser de `MidiGenPlay/Tools~/surprise`. Se ejecuta con `python -I -X utf8 export_listen.py <tool_dir> <wrapped_data.json> <candidates.txt> listen_clips.json <asset_dirs>…`.
- `listen_clips.json`: las muestras con los números del modelo.
- `template.html` + `build.py` → `index.html`, la página publicada.
- **Resultados:** colección `respuestas` del db del artefacto, un documento por persona. Si alguien no puede escribir (por ejemplo, alguien de fuera con acceso de solo lectura), copia el JSON al final de la prueba y se lo manda a Claudio.

No es el sonido de MidiGenPlay. Para tener fidelidad, renderiza las mismas progresiones con *MidiGenPlay → Smoke → Composition Smoke* (`.mid`).

## Cómo se crean los assets (decisión del 2026-11-02)

- **Progresiones nuevas:** el agente prepara el bloque de importación (skill `chord-progression-importer`) y Claudio lo pega en el Import del Chord Progression Editor, que crea y valida el asset.
- **Paletas:** el agente edita el YAML directamente usando los GUID de los `.meta` existentes. Si una paleta lleva una progresión nueva, primero hay que refrescar Unity para que exista su GUID.
- **Cartas:** van después de la D3. El agente escribe la ficha y Claudio la crea en el Card Editor, o se hace en varias pasadas con refrescos de Unity.

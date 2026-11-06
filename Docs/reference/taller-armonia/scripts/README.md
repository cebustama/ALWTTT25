# Scripts de la revisión del modelo de 12tone (2026-11-02)

Los escribió el agente que hizo de Music Lab en la crítica `../revision-music-lab-12tone-2026-11-02.md`. Se conservan para poder repetir los resultados.

- `taller_core.js`: copia del código de teoría de `../template.html` (parser y `arrange` v3) extraída en ese momento. Si cambia `template.html`, hay que volver a extraerla.
- `shape.js`: calcula la forma de un bucle según el modelo de 12tone: 4 casillas por tiempo, fronteras fuertes (F) o débiles (D), anclas y tipo. Lo aplica a los clips de `../../prueba-escucha/listen_clips.json` antes y después de los niveles. Uso: `node shape.js`.
- `loops12.py`: piloto sobre McGill Billboard 2.0. Detecta los bucles, los parte en casillas y cuenta los tipos y los destinos por posición, separando los acordes sostenidos de las terceras reales. Uso: `python -I loops12.py <ruta a McGill-Billboard>`. El corpus está en la caché de `MidiGenPlay/Tools~/surprise/.cache/dl/McGill-Billboard`.

Es un piloto y no una herramienta gobernada. Si se adopta, la versión buena va a `MidiGenPlay/Tools~/surprise/loop_shape.py`.

## Prueba de oro de las perillas (2026-11-06)

- `oro.js`: carga la teoría de `../template.html` (sin copiarla) y corre `arrange` sobre el catálogo de `../index.html`. `escribir oro-v3.json` guardó la instantánea de la v3 antes de sacar las perillas; `comparar oro-v3.json` exige salida idéntica con las perillas de fábrica; `perillas` imprime ejemplos de las perillas nuevas. Correr `python -I -X utf8 build.py` antes.
- `oro-v3.json`: la instantánea (792 casos). No se regenera salvo que cambie la v3 a propósito.
- Nota: `taller_core.js` es una copia anterior a las perillas.

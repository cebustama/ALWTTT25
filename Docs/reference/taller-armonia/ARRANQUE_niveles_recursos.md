# Arranque · Los niveles del taller aprovechan los recursos de Music Lab (P1–P5)

> **Hecho el 2026-11-11.** Pasos 1–6 cerrados: v4 con P1–P4 encendidas por la hoja de oído (resultados en `revision-music-lab-reglas-P1-P5.md` §13), `scripts/oro-v4.json`, taller republicado. Lo que queda (prompt de `armonia.ajustar`, copia web, P6) está en ese §13.

Preparado el 2026-11-11. Prioridad 1 de Claudio en la línea armónica (no toma sesiones de la demo). Ábrelo en una sesión desde `D:\Projects\ALWTTT25` (o desde la raíz, siguiendo este archivo).

## Para qué

Los niveles 2 y 3 del taller usan 6 de los 13 recursos armónicos validados de Music Lab, 2 a medias, no usan 3 y tienen 2 bloqueados. Lo que falta es lo más frecuente en el repertorio (el ii–V completo, la cadena de dominantes, el disminuido de paso). Objetivo: añadirlo sin romper lo que ya suena bien, y dejarlo listo para portarlo a MidiGenPlay (prioridad 2, después de la demo).

## Leer antes

- `revision-music-lab-recursos-2026-11-11.md` (este directorio): el cruce, las propuestas P1–P6, el proceso estándar (§4) y el paso a MidiGenPlay (§5). Página: https://claude.ai/artifact/Tv4i2zrJWghR5csxrhqCwg
- `template.html`, sección `levels` (perillas `PERILLAS_V3`, `arrange()`), y `README.md` (prueba de oro).
- `D:\Projects\Music Lab\recursos-armonicos.md` (nota de cabecera del 2026-11-11) y las fichas de `D:\Projects\Music Lab\Biblioteca\jazz\` que se citan como ejemplos.

## Pasos (proceso estándar del §4)

1. **Reglas, con criterio de Music Lab.** Para P1–P5, escribir en `revision-music-lab-reglas-P1-P5.md`: dónde va cada una (qué acorde, qué mitad), en qué modos, qué gasta del presupuesto, en qué orden respecto a las demás, y 2–3 casos de la Biblioteca con su compás. Si hay dudas musicales, se preguntan a Claudio (regla: las dudas musicales se consultan a Music Lab).
2. **Perillas nuevas, apagadas.** Implementar cada una detrás de su perilla, con el valor de la v3 por defecto. `node scripts/oro.js comparar oro-v3.json` tiene que seguir dando **792 idénticos**.
3. **Prueba de reducción.** Script nuevo (`scripts/reduccion.js`): toma secciones de la Biblioteca (las entradas `origin: "musiclab"` del catálogo), las reduce a su esqueleto diatónico, les pasa los niveles con cada perilla encendida y mide cuánto se parecen a la sección real (acordes por compás, como `validar-ficha.mjs`). Elegir 6–10 secciones donde aparezcan los recursos.
4. **Prueba de oído.** Hoja de pares A/B como la de la v3 (`hoja-prueba-oido.html`); Claudio decide qué se enciende y en qué nivel.
5. **Si algo se enciende:** `oro.js escribir oro-v4.json`, README al día, `build.py` y publicar el taller (https://claude.ai/artifact/6ABsXYisfqmwdof7PhsYik; también la copia web con `--web`, si Claudio quiere).
6. **Cierre:** actualizar la tabla del §2 de la revisión, `_comun/foco.md` y el arranque de la prioridad 2 en MidiGenPlay (`Documentation~/planning/active/Sesiones_MidiGenPlay.md`, fila «Niveles armónicos»).

## Hecho cuando

P1–P5 implementadas detrás de perillas, oro-v3 idéntico, prueba de reducción con resultados, prueba de oído hecha y decisión de Claudio sobre qué se enciende. P6 (perfiles de estilo) puede quedar para la sesión siguiente.

## Fuera de esta sesión

Pivote I → ii, bajo cromático y cliché de línea menor (bloqueados por MidiGenPlay); el port a C#; las canciones largas que suenan mal (§6 de la revisión, menor).

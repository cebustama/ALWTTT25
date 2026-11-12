# Arranque · P6: proteger la identidad y que la semilla elija el adorno (con pesos)

Preparado el 2026-11-12. Línea armónica (foco acotado de Claudio; no toma sesiones de la demo). Se abre desde `D:\Projects\ALWTTT25`.

## Para qué

La prueba de oído de las semillas (§17 de `revision-music-lab-reglas-P1-P5.md`) dio dos resultados:

1. **La identidad aguanta en 3 de 4 cartas, pero la cadencia andaluza se rompe.** Las semillas acortan el ♭VI y meten ♭viiø7 – II7 antes del V, y así cortan el medio tono ♭VI → V, que define la carta.
2. **La variedad se queda corta** (decisión de Claudio). Hoy la semilla decide dónde caen los adornos, pero no cuáles.

## Leer antes

- `revision-music-lab-reglas-P1-P5.md`: §15, §16 y **§17**, incluido el aviso final sobre la regla B.
- `template.html`, sección de teoría: `arrange(..., seed)`, `hash32`, `remateDe`, el bucle `each(...)` de las reglas y `PERILLAS_V3`.
- `scripts/oro.js`, `oro-v4.json` (sin semilla) y `oro-v5.json` (con semillas).
- `prueba-oido-semillas/` (hoja, fuente y build): base para la hoja de esta sesión.

## Lo ya decidido

- **D1 = B (Claudio, 2026-11-12):** una regla general que protege el paso de bajo que define la carta, no anclas marcadas a mano. Music Lab la confirma y la acota antes de implementarla. Si falla, A (anclas por carta) como respaldo.
- La vuelta 1 sigue de fábrica en el taller. En el juego, «de fábrica» significa la primera vez que suena la carta; si se vuelve a tocar, puede variar. Va al port, no a esta sesión.
- Sin semilla = v4. Mientras se pueda, la regla nueva no debería cambiar `oro-v4`. Si la regla acotada lo cambia, es decisión de Claudio y da un `oro-v6`.
- D11: el blues (I7, IV7) será un perfil, no una perilla suelta.

## Decisiones para presentar al abrir (formato D, en lenguaje llano)

1. **La regla de identidad, acotada.** Llevar a Music Lab, con un subagente, el caso de la andaluza y tres o cuatro progresiones con bajada (lamento i – ♭VII – ♭VI – V, Pachelbel, Frigio descenso, *Hit the Road Jack*). Opciones a comparar:
   - proteger solo el medio tono;
   - proteger el último paso hacia el V o hacia la tónica;
   - proteger toda la bajada.

   Recomendar la que no cambie la de fábrica.
2. **Qué adorno elige la semilla (c).** Entre los adornos encendidos que caben en un hueco (ii–V, disminuido, menor prestado, sus4…), la semilla elige con pesos, en lugar de seguir la prioridad fija. Opciones:
   - pesos iguales;
   - pesos por nivel;
   - pesos por perfil de estilo (pop, jazz, blues).

   Recomendado: por nivel ahora y los perfiles después, para que la hoja de oído sea pequeña.
3. **Cuánta variedad más.** Además de (c), ¿se sube el presupuesto con la intensidad, o el remate al azar debe oírse también en cartas que cierran solas? Medir con `oro.js` cuántos casos cambian antes de decidir.

## Pasos

1. Consultar a Music Lab sobre la regla de identidad (paso 1 de las decisiones) y escribir las reglas en §18.
2. Implementar en `template.html`. Sin semilla, `oro-v4` idéntico salvo que la regla de identidad decida otra cosa (en ese caso, `oro-v6` con permiso de Claudio). `oro-v5`: rehacerlo, porque las semillas cambian a propósito.
3. Medir la variedad: casos que cambian por nivel con 4 semillas, antes y después; y la andaluza con s1, s2 y s6 ya sin el ii–V antes del V.
4. Hacer una hoja de oído a ciegas con el formato de `prueba-oido-semillas/`: las mismas 4 cartas y 1–2 nuevas con bajada, con las semillas fijas.
5. Cierre: README, `build.py`, publicar el taller (https://claude.ai/artifact/6ABsXYisfqmwdof7PhsYik), la fila «Niveles armónicos» de `Sesiones_MidiGenPlay.md`, `_comun/foco.md` y `_comun/proyectos.md`. Commits solo con pathspec.

## Hecho cuando

- La andaluza con semilla mantiene el ♭VI → V (comprobado en el oro y a ciegas).
- La semilla elige el adorno, con pesos documentados en §18.
- Claudio dice a ciegas que la variedad alcanza y que las cartas siguen sonando a sí mismas.

## Otro arranque listo en la misma línea

`ARRANQUE_narrador_forma.md` (narrador musical: secciones y finales). Estaba previsto para después de esta prueba de oído; Claudio elige cuál va primero.

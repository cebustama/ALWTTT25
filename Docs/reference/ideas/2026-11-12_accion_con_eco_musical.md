# Idea: cada carta de acción suena (eco musical de la acción)

**Origen:** Claudio, 2026-11-12, jugando los smokes de S9 (REINA-1). Idea suya; sin decidir ni planificar.

## La idea

Cuando se juega una carta de **acción**, además de su efecto de juego, suena algo en el loop que está sonando. La acción se oye.

Ejemplos de Claudio:
- **Foco** sobre Robot C2: un fill especial de batería al final del loop.
- **Subidón Estático**: un adorno de bajo de Conito.

En sus palabras: «conexión entre acción y reacción».

## Por qué importa

- Hoy las cartas de acción mueven números (Stress, Composure, estados) en silencio. Solo las cartas de composición cambian la música. Con esta idea, el jugador oye que su jugada tuvo efecto, que es justo lo que pide la demo: que se entienda sin explicarla.
- Encaja con «las cartas son paletas»: cada acción sería una pequeña paleta (fill, adorno, acento), con variedad por semilla y dentro de una identidad.

## Con qué conecta

- **MidiGenPlay, fills y acentos** (MGP-DRUM-FILL-1 y MGP-DRUM-ACCENT-1, 2026-11-12): ya existen fills de biblioteca y procedurales, sorteados por hash. Un «fill pedido» al final del loop es el primer consumidor natural. El bajo ya sigue los acentos (MGP-BASS-ACCENT-1): un adorno de bajo pedido por una carta va por el mismo camino.
- **Combos de banda** (`2026-11-05_combos_de_banda.md`): un combo podría tener su propio eco musical, más grande que el de una carta suelta.
- **Frontera ALWTTT ↔ MidiGenPlay**: ALWTTT decide *qué* pide cada carta (significado de juego); MidiGenPlay decide *cómo* suena. Habría que añadir a la frontera una petición del tipo «adorno en este loop», no tocar el paquete desde ALWTTT.

## Preguntas abiertas

1. ¿Suena en el loop actual (inmediato, hay que inyectarlo en la parte que ya está sonando) o en el siguiente (más fácil: entra con la regeneración del loop)?
2. ¿Un eco por carta, o por familia de cartas (Foco → fill, buffs → adorno, curas → acorde)?
3. ¿Cómo encaja con AUDIO-LOOP-1 (S9b), que toca justo el paso de un loop al siguiente?

## Estado

Idea guardada. Candidata para después de la demo, o como sesión pequeña si sobra hueco (D-PLAN por decidir).

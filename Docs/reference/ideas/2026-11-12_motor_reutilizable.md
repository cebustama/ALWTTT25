# Un «motor» reutilizable sacado de ALWTTT

Idea de Claudio, 2026-11-12: encapsular el código genérico de videojuegos de ALWTTT como esqueleto
reutilizable para otros juegos en Unity, y así crear juegos nuevos más rápido. Estado: **idea, para
después de la demo** (D-PLAN-18). No es autoridad.

## Qué podría salir (✦ IA, a comprobar pieza por pieza)

Piezas de ALWTTT que no dependen de la música ni de las cartas:
- El **bus de eventos** (`SensoryEventBus`) y el patrón de oyentes que solo leen.
- El **Dev Mode** con pestañas, y el registro de ajustes de F5 cuando exista (D-PLAN-17).
- La **telemetría** en JSON Lines por partida.
- **Tooltips y glosario de conceptos** (palabras clave con su ficha, ES/EN).
- Los **efectos de estado** como catálogo de ScriptableObjects (tick, duración, apilado).
- La **importación de contenido desde JSON** en el editor (el elenco de S7).
- El **flujo de escenas**: menú, hub, mapa, encuentro, resultado.

## Precedentes en los proyectos de Claudio

- **LLM Interaction Core** (`com.bcs.llm-core`) ya es un paquete compartido entre ALWTTT, Islands y Eon:
  el mismo camino (paquete UPM en su propio repo) vale para un «game kit».
- **Islands.PCG** ya es un paquete con su anfitrión aparte.
- **mundo-islas** se portará a un proyecto de Unity 6 nuevo: sería el primer consumidor natural.

## Cómo hacerlo sin romper ALWTTT (✦ IA)

1. Inventario: qué clases no importan nada de `ALWTTT.Music`, `Cards` ni MidiGenPlay.
2. Sacar de una en una a un paquete local, con su propio namespace y sus tests. ALWTTT lo consume y su suite
   sigue verde.
3. Un proyecto vacío que solo use el paquete demuestra que es independiente.

Riesgo: extraer antes de tener un segundo juego que lo use suele dar abstracciones equivocadas. Mejor
extraer cuando arranque el segundo proyecto, empezando por lo que ese proyecto necesite.

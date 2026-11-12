# Bifurcación en el mapa y editor de giras (MAPA-2)

**Fecha:** 2026-11-12 (S7, ELENCO-1). **Estado:** decisión de Claudio de meterla en la demo; D4 = B
(en el hueco de S11, PULIDO-ARTE, quitando «escenario por nodo»). Requiere la enmienda **D-PLAN-6** de
REPLAN-ART D1 («sin ramas»), por escribir en el plan al cerrar S7. Registro, no autoridad.

## Qué

- **Bifurcación:** tras el gig 1, el jugador elige entre dos gigs (p. ej. «Tentáculos» y «El jefe y su
  banda») viendo a quién se enfrenta. Al elegir, el camino no elegido se desdibuja (los puntos se apagan) y
  su nodo queda bloqueado: no se puede volver atrás. Después, los caminos se juntan en la Reina.
- **Por qué (Claudio):** añade rejugabilidad, es una funcionalidad básica del diseño y es reutilizable.
- **Editor de giras:** una ventana (menú ALWTTT) para editar los nodos de una gira (gig, título, posición,
  conexiones) sin navegar entre inspectores y listas anidadas. Más adelante: editar las condiciones de
  generación del grafo.

## Qué cambia en el código (estimación, sin leer a fondo)

- `LinearMapController` pasa de una lista a un grafo: cada nodo con sus siguientes.
- El estado de la partida guarda el camino elegido (hoy el progreso es un índice).
- El camino punteado se dibuja por arista; las aristas descartadas se apagan.
- La ventana del editor escribe en el mismo componente (o en un SO de gira, a decidir en la sesión).

## Para la sesión

Ejecutar `/buscar-referencias --all` sobre mapas con ramas (Slay the Spire, Monster Train, FTL).

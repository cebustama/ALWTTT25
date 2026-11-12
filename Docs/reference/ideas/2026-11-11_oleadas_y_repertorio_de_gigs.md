# Oleadas y repertorio de gigs (2026-11-11)

**Tipo:** idea / referencia (no autoridad). Surge al abrir S7 (ELENCO-1), respuesta de Claudio a D1.

## Lo que dijo Claudio

> «No sé si me parece demasiado 4 personajes con tantas mecánicas en un solo gig. Diseñaría un gig SOLO
> con el tentáculo, para probar, y otro con el Jefe y sus secuaces. Después mezclamos. Yo diseñaría
> varias gigs (no solo 2) y luego las asignamos o escogemos random. Lo importante es diseñar un par que
> estén bien pensadas y probadas. También está la opción de implementar la idea de "waves" y así
> probamos con muchos enemigos a la vez.»

## 1. Repertorio de gigs (entra en S7)

- Cada gig enseña **una** cosa; mezclar viene después, cuando cada pieza esté probada.
- Se diseñan varios encuentros (`GigEncounterSO`) y luego se asignan a los nodos del mapa o se sortean.
- Primer repertorio (S7): «Ranas y Rayas», «Tentáculos» (solo), «El jefe y su banda».
- Sorteo por nodo: **después** de probarlos; hoy cada nodo del mapa lineal tiene un `launchConfig` fijo.

## 2. Oleadas (no entra en S7)

El público llega por tandas dentro del mismo gig: cuando una tanda queda convencida (o tras N loops),
entra la siguiente. Permite probar muchos enemigos sin llenar el escenario a la vez.

- **Coste:** sistema nuevo. Hoy el público se crea una vez en `GigManager.BuildAudience` y hay 4 posiciones;
  haría falta aparecer miembros a mitad de gig (lo mismo que pide «dividirse en dos» de Rayas, Phase C en
  `2026-10-30_publico_con_rol.md` §1), condición de victoria por tandas y legibilidad («quedan 2 tandas»).
- **Referencias:** Monster Train (oleadas por piso, con aviso de la siguiente); Darkest Dungeon (refuerzos
  que ocupan huecos de la fila).
- **Dónde encaja:** después de la demo, o como deseo si sobra tiempo. Comparte código con «dividirse».

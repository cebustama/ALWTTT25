# Demo del 11-dic — plan por sesiones (PROPUESTA, v2)

**Estado:** propuesta del 2026-10-30, sin aceptar. Hasta que Claudio la acepte manda la tabla de
REPLAN-DIAG en `S5_DemoCutClose_Sub_Roadmap.md`. Si se acepta, REPLAN-DIAG recibe una línea que apunta
aquí, y D-PLAN-0 enmienda REPLAN-ART D1 («sin ramas, tienda ni terapia»).
**v2 (misma tarde):** nueva filosofía de Claudio: la demo es el **esqueleto del juego real**, montado con
todo el arte que ya existe, para que se vea el avance y se imagine la experiencia final. Entra la nave
como centro, el elenco con sprites nuevos, los gritos y la Reina con su sprite.
**Criterio de la demo (D-ART-2):** se entiende sin explicarla: cómo se juega, cómo se gana, cómo se
pierde. Lo nuevo no lo sustituye: lo que no esté hecho lleva «EN DESARROLLO» y un «Volver», para que
nadie se atasque ahí.

## El esqueleto (lo que verá quien juegue)

```
Menú → NAVE (plano con salas)
          ├─ Dormitorio: elegir alineación (arte «Personajes»)            → funciona
          ├─ Cabina del capitán (Sion): mapa de Asteroide City            → funciona (mapa lineal)
          │     └─ gig 1 → recompensa («Escoge tu carta») → nave
          │     └─ gig 2 → … → Reina Gusano → mapa polinizado
          ├─ Chatarrero: tienda / «abre el show»                          → EN DESARROLLO
          ├─ Sala de ensayo, recreación, cocina, jardín, baño            → EN DESARROLLO
```

Arte disponible (`Assets/Resources/Sprites/Ship/`): plano de la nave con salas numeradas
(`image (5).png`) y la nave entera; sala de ensayo, sala recreativa, chillout y vendedor a pantalla
completa. En el plano aparecen además las pantallas de mapa, «Escoge tu carta», «Personajes» (Zit,
Conito, Sivi, A-23), el capitán Sion y las libreas (la tripulación). **Pedir a Matías los archivos
sueltos** de esas pantallas y de la pantalla de elegir con el bicho en el foco: hoy solo están dentro del
plano o en capturas.

## Supuestos

- **3 sesiones por semana** de unas 2–3 horas. Si son menos, se recorta por el orden de abajo; no se
  alargan las sesiones ni se trabaja de madrugada.
- Cada sesión de código trae su guion de partida (máx. 3 smokes humanos; regla de REPLAN-DIAG).
- La nave nueva es una **escena fina sobre un PNG** con zonas clicables, como `LinearMapController`
  (MAP-DEMO-1). No reutiliza `2 - Ship Interior` (ensayo con composición), que queda para Phase C.
- Congelación de arte: vie 20-nov. Congelación de funcionalidades: vie 27-nov.

## Sesiones

| Semana | # | Sesión | Objetivo único | Lo has logrado si… | M |
| --- | --- | --- | --- | --- | --- |
| 2–6 nov | S1 | **COH-1, cierre** | Jugar el guion de `COH-1_Guion.md` y pegar el `Editor.log` | Pasos 1–9 coinciden con el log; commit | M3 |
| | S2 | **GIG-UI-1** | Montar la consola del manager según su runbook | La cohesión se ve en el HUD y la consola se lee | M3 |
| | S3 | **LEGIB-1a: ver antes de jugar** | Al pasar el ratón por una carta, el Big Num muestra «Y → Z» | El número previsto es el que pasa (3 cartas distintas) | M1 |
| 9–13 nov | S4 | **LEGIB-1b: lo que recibes** | La intención del público dice qué hará y a quién (línea al objetivo); sin textos internos («AddVibe») | Una persona ajena dice qué hará cada miembro del público | M1 |
| | S5 | **NAVE-1** | Escena de la nave: plano con salas clicables; cada sala abre su arte con «EN DESARROLLO» y «Volver»; la cabina abre el mapa | Menú → nave → cabina → gig → vuelta a la nave, sin romper la partida | — |
| | S6 | **NAVE-2: dormitorio = alineación** | En el dormitorio se elige la banda: 2, 3 o 4 músicos | Cada opción arranca con esa banda y su mazo; sale en la telemetría | M2 |
| 16–20 nov | S7 | **ELENCO-1** | Los 9 sprites listos entran como público; gigs 1 y 2 con sus grupos (`reference/ideas/2026-10-30_publico_con_rol.md` §7) y el objetivo «el líder» | Los dos gigs tienen público propio que se ve distinto y actúa distinto | M4 |
| | S8 | **GRITOS-1** | Bocadillos con frases por habilidad; `{músico}` se sustituye por el objetivo | Antes de cada golpe se lee quién lo recibe; alguien se ríe | M1 |
| | S9 | **REINA-1** | Reina Gusano con su sprite y huevo · huevo · eclosión | En la prueba, alguien explica qué hace la Reina sin ayuda | M4 |
| 23–27 nov | S10 | **ALINEACIÓN-2 + TEST-SETUP-1** | Tutorial y mazo coherentes con cada alineación; GigSetup desde el menú | Las 3 alineaciones juegan el gig 1 sin diálogos de músicos ausentes | M2 |
| | S11 | **PULIDO-ARTE** | Recompensa con el arte de «Escoge tu carta», escenario por nodo, mapa polinizado al vencer a la Reina | Se ve en una partida completa | M5 |
| | S12 | **Congelación** (vie 27-nov) | Build interna + guion de la prueba M6 | La build arranca desde un `.exe` limpio | M7 |
| 30 nov–4 dic | S13–S15 | **Prueba M6** | ≥3 personas externas sin explicar; se mira dónde se atascan y se les pide que cuenten cómo se gana y cómo se pierde | Cada una lo sabe decir; solo se arreglan bloqueantes | M6 |
| | | Build candidata (vie 4-dic) | | | M7 |
| 7–11 dic | S16 | **Discord** | Texto, capturas, publicación (vie 11-dic) | Publicada | M7 |

## Orden de recorte si vamos tarde

Se recorta de arriba abajo; nunca M1, M3, M6:
1. **Poses de enemigos** (fuera del plan; cambiar de sprite por estado, ~1 sesión, si sobra).
2. **PULIDO-ARTE** (S11): queda la recompensa de siempre y un solo escenario.
3. **GRITOS-1** (S8): solo frases genéricas, sin `{músico}`.
4. **ALINEACIÓN** (S6, S10): banda fija; el dormitorio pasa a «EN DESARROLLO».
5. **NAVE-1** (S5): la cabina se queda; las demás salas, una sola pantalla con el plano.

## Deseos (no entran salvo que sobre tiempo)

- Poses por estado del público (solo el secuaz 1 las tiene).
- Chatarrero funcional: «abre el show», la mejora inicial de la partida (pantalla del bicho en el foco).
- Interacciones musicales, convencidos que cantan, «boooring» entre loops (REPLAN-ART D3 = B).

## Decisiones abiertas

- **D-PLAN-0:** la demo como esqueleto del juego (nave + salas en desarrollo) en lugar de solo combate.
- **D-PLAN-1:** dónde se elige la alineación (A: dormitorio / B: pantalla del bicho en el foco / C: banda fija).
- **D-PLAN-2:** salas en desarrollo (A: todas las que tienen arte / B: solo chatarrero y sala de ensayo).
- **D-PLAN-3:** público con rol en la demo (A: nada / B: grupos con datos + objetivo visible + gritos / C: disparadores).
- **D-PLAN-4:** ritmo de 3 sesiones por semana.

# Demo del 11-dic — plan por sesiones (PROPUESTA)

**Estado:** propuesta del 2026-10-30, sin aceptar. Hasta que Claudio la acepte manda la tabla de
REPLAN-DIAG en `S5_DemoCutClose_Sub_Roadmap.md`. Si se acepta, REPLAN-DIAG recibe una línea que apunta
aquí y este archivo pasa a ser la secuencia vigente.
**Para qué:** REPLAN-DIAG fija **qué** falta (M1–M7) y una tabla por semanas; no dice qué se hace en
cada sesión ni cuándo está terminada. Este plan lo baja a sesiones de un solo objetivo, con su criterio
de «lo has logrado si…», y añade lo nuevo del 2026-10-30 (alineaciones, escaparate de tienda y nave,
público con rol).
**Criterio de la demo (D-ART-2):** se entiende sin explicarla: cómo se juega, cómo se gana, cómo se pierde.

## Supuestos

- **3 sesiones por semana** de unas 2–3 horas. Si son menos, se recorta por el orden de abajo; no se
  alargan las sesiones ni se trabaja de madrugada.
- Cada sesión de código trae su guion de partida (máx. 3 smokes humanos; regla de REPLAN-DIAG).
- Dependencias de arte (Matías): icono de nodo y nave; arte de la pantalla de elegir (la del bicho en el
  foco con 3 opciones: **no está en el repo**, solo la captura); sprites finales de los enemigos de la
  demo; Reina Gusano. **Congelación de arte: vie 20-nov.**

## Sesiones

| Semana | # | Sesión | Objetivo único | Lo has logrado si… | M |
| --- | --- | --- | --- | --- | --- |
| 2–6 nov | S1 | **COH-1, cierre** | Jugar el guion de `COH-1_Guion.md` y pegar el `Editor.log` | Pasos 1–9 coinciden con el log; smokes 1–2 PASS; commit | M3 |
| | S2 | **GIG-UI-1** | Montar la consola del manager según su runbook | Se ve la cohesión en el HUD del gig y la consola se lee (smoke 1 de GIG-UI-1) | M3 |
| | S3 | **LEGIB-1a: ver antes de jugar** | Al pasar el ratón por una carta, el Big Num muestra «Y → Z» | El número previsto es el que pasa al jugarla (3 cartas distintas) | M1 |
| 9–13 nov | S4 | **LEGIB-1b: lo que recibes** | La intención del público se lee: icono con número y a quién va; sin textos internos («AddVibe») | Una persona ajena dice qué hará cada miembro del público el próximo turno | M1 |
| | S5 | **LINEUP-1: elegir alineación** | Pantalla al empezar la partida con 3 opciones: 2, 3 o 4 músicos (arte de la pantalla de elegir) | Cada opción arranca la partida con esa banda y su mazo; la elección sale en el log de telemetría | M2 |
| | S6 | **LINEUP-1b + TEST-SETUP-1** | Tutorial y mazo coherentes con cada alineación; GigSetup accesible desde el menú del build | Las 3 alineaciones juegan el gig 1 sin diálogos que hablen de músicos ausentes | M2 |
| 16–20 nov | S7 | **REINA-1** | Reina Gusano con 1–2 mecánicas que se lean | En la prueba, alguien explica qué hace la Reina sin ayuda | M4 |
| | S8 | **PUBLICO-ROL-1** | Dos grupos de público que funcionan juntos, con lo que ya existe + el objetivo visible de la intención | Se ve quién ayuda a quién (ver `reference/ideas/2026-10-30_publico_con_rol.md`) | M4 |
| | S9 | **ARTE-IN** | Sprites nuevos de enemigos y 3 escenarios, uno por nodo | Los 3 gigs tienen escenario y público propios | M5 |
| 23–27 nov | S10 | **MAPA-INFO** | Pasar el ratón por un nodo muestra quién hay en ese gig y qué le gusta; polinización al vencer a la Reina | Antes de entrar, se sabe a qué público vas | M5 |
| | S11 | **ESCAPARATE-1** | Tienda de cócteles y nave accesibles desde el mapa, con su arte, «EN DESARROLLO» y «Volver» | Se entra y se vuelve al mapa sin romper la partida | — |
| | S12 | **Congelación** (vie 27-nov) | Build interna + guion de la prueba M6 | La build arranca desde un `.exe` limpio | M7 |
| 30 nov–4 dic | S13–S15 | **Prueba M6** | ≥3 personas externas, sin explicar; se mira dónde se atascan y se les pide que cuenten cómo se gana y cómo se pierde | Cada una lo sabe decir; solo se arreglan bloqueantes | M6 |
| | | Build candidata (vie 4-dic) | | | M7 |
| 7–11 dic | S16 | **Discord** | Texto, capturas, publicación (vie 11-dic) | Publicada | M7 |

## Orden de recorte si vamos tarde

Se recorta de arriba abajo, nunca M1, M3, M6:
1. **Poses de enemigos** (ya fuera del plan; ver abajo).
2. **ESCAPARATE-1** (S11): se puede sustituir por una captura de la tienda y la nave en el texto de Discord.
3. **PUBLICO-ROL-1** (S8): quedarse con un grupo.
4. **MAPA-INFO** (S10): dejar solo la polinización.
5. **LINEUP-1** (S5–S6): banda fija; la elección pasa a TEST-SETUP-1.

## Deseos (no entran salvo que sobre tiempo)

- **Poses por estado del público** (reposo, habilidad, convencido): solo un enemigo las tiene
  (`image.png`, 4 poses del secuaz 1). Encaje mínimo: cambiar de sprite por estado; ~1 sesión.
- Interacciones musicales, convencidos que cantan, «boooring» entre loops, malas notas, variedad
  musical (REPLAN-ART D3 = B: después de la demo).

## Decisiones abiertas (para abrir S5, S8 y S11)

Ver la respuesta del 2026-10-30; se registran aquí cuando Claudio las cierre.
- **D-PLAN-1:** alineaciones en la demo (A: 3 opciones al empezar / B: solo en modo prueba / C: fija).
- **D-PLAN-2:** acceso al escaparate (A: nodo Comerciante / B: botón Nave / C: ambos).
- **D-PLAN-3:** público con rol en la demo (A: nada / B: 1–2 grupos con lo existente / C: sistema de disparadores).
- **D-PLAN-4:** ritmo de 3 sesiones por semana.

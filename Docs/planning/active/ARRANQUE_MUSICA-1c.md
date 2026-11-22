# Arranque MUSICA-1c · Rise Up, pasada por el mazo y ajustes del bajo

Escrito el 2026-11-22. Planning, no autoridad. Continúa MUSICA-1b, cerrada el 2026-11-22 (el bajo y los
acordes que se escuchan). El lunes 23-nov sigue el resto del plan de ALWTTT; esta sesión va donde
quepa antes de la congelación del 27-nov. Si no cabe, pasa a POST-DEMO B10.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 MUSICA-1c — Rise Up, pasada por el mazo y ajustes del bajo.

Lee Docs/planning/active/ARRANQUE_MUSICA-1c.md. Antes, comprueba el calendario (lo del plan
DEMO-1211 que tenga fecha va primero). D2 = A, D3 = A y D4 = A siguen vigentes.

Viene de MUSICA-1b (2026-11-22): Wormus Major gira en el fill (HoldThenTurn N = 4); el bajo lee
el mismo horario y también gira; Conito tiene 4 cartas de bajo con paleta por nivel (Root Line,
Slap Groove, Walking Bass, Ballad Hold; MGP-BASS-PALETTE-1, una variante por canción). El Banco
acepta variant=, sched=, phrase=, pstyle=, ptech=, psnare=, pdead= y pseed= en las recetas.

Pide, en este orden:
1. Ajustes del bajo (de oído, MUSICA-1b): volumen de todos los bajos ×1,2; el slap más fuerte;
   Conito prefiere Slap Bass 2 a Slap Bass 1; Ballad Root alterna raíz y raíz–quinta; Ballad Hold
   exagerado con adornos (no en todos los compases); Walking que gira necesita algo más para
   ser «avanzado».
2. Rise Up (melodía en modo figura, ajustes de TEST_MelodyFigure_StyleBundle) frente a Showtime.
3. Pasada por el mazo de la demo: BPM de cada carta y rangos de tempo de sus patrones.
4. Smoke en el juego: una canción con Default Mode, Wormus Major, Root Line y Rise Up.

Cada prueba de oído va con sus recetas A y B completas, listas para pegar, y una línea de en
qué fijarse.

Restricciones: logs con la etiqueta [MUSICA-1]; no escribir .meta; decisiones en formato D;
≤3 smokes jugados por Claudio, de oído; pasos de editor uno a uno; sin commit ni push sin pedirlo.
```

## Estado al cerrar MUSICA-1b

| Pista | Carta | Qué suena |
| --- | --- | --- |
| Ritmo | Default Mode | fill cada 4 compases, crash después, FillLibrary |
| Acordes | Wormus Major (Core Major) | AttacksAndGaps; Random con HoldThenTurn N = 4: figura A tres compases y B en el del fill |
| Bajo | Root Line (inicial ×2) | paleta Root Pump (básico) · Root Five (medio) · Root & Fills (avanzado) |
| Bajo | Slap Groove (inicial ×1) | Disco Octaves · Slap Groove (variantes en los compases 2 y 4) · Super Slap · Funk Slap Phrases |
| Bajo | Walking Bass (recompensa) | Walk Triad · Walking · Walking que gira |
| Bajo | Ballad Hold (recompensa) | Ballad Root · Ballad Half (R‑5 en blancas) · Ballad Hold |
| Melodía | Rise Up | **sin encender todavía** (esta sesión) |

Menús: *ALWTTT ▸ Music ▸ MUSICA-1b ▸ Crear bajos nuevos de Conito* y *▸ Organizar los bajos de
Conito en 4 cartas* (idempotentes). Análisis visual: https://claude.ai/artifact/UEoCAfk5hmH8sZmDKv9UBv

## Recetas de referencia

Base para el bajo (batería disco, 108 BPM):

```
bench bpm=108 card=starter_default_mode pattern=FF_DiscoPulse
chords card=starter_wormus_major who=Sibi
bench progression=Prog_Maj_AxisRot_8c
bass card=conito_root_line who=Conito variant=RootFive
```

Balada: `bpm=80` y `pattern=FF_HalfTimeHeavy` (las frases necesitan huecos en el bombo).

## Pendiente fuera de esta sesión

- Niveles de pista: la paleta ya etiqueta cada variante (básico, medio, avanzado); falta que ALWTTT
  pase el nivel (`Design_Track_Card_Levels_v0_1.md`). Con un nivel concreto, el sorteo es por loop.
- Phrases con técnica slap: más variedad de pops cuando el bombo es escaso (solo probado con DiscoPulse).
- Reggae, tumbao y Motown necesitan motor (grado por paso en el pocket; frases melódicas).
- El 2.º solista armoniza al 1.º (POST-DEMO B9, con DUET-1).

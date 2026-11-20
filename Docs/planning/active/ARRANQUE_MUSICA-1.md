# Arranque MUSICA-1 · la variedad musical se oye en el juego

Escrito el 2026-11-19 al cerrar COMBOS-1. Planning, no autoridad. Fila 8 del «Orden de entrada» de
`DEMO-1211_Plan_de_Sesiones.md` (D-PLAN-12 = A). Si no llega antes del 27-nov, lo que no suene bien se queda
apagado (pasa a B de POST-DEMO).

**Comprobación de calendario antes de empezar.** Antes de la congelación (S12, vie 27-nov) tienen que caber
**S10** (imprescindible) y S11. Si al abrir quedan menos de 3 sesiones hasta el 27-nov, haz **S10 primero** y
MUSICA-1 se reduce a D-GF y a la pasada de escucha de las cartas de la demo.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 MUSICA-1 — la variedad musical se oye en el juego.

Lee Docs/planning/active/ARRANQUE_MUSICA-1.md y haz MUSICA-1. Antes, comprueba el calendario
(S10 tiene que caber antes del 27-nov). Empieza con /buscar-referencias --all (variedad en la
música generativa de juegos, solos sobre la voz, cómo suena «la banda que se escucha») y
preséntame las decisiones en formato D antes de construir, empezando por D-GF.

Contexto: COMBOS-1 cerrada el 2026-11-19 (combos con nombre en la previsión, Capas y Gran cierre,
previsión antes de Play, mano inicial 4 / robo 2 en F5). Hechos en MidiGenPlay y sin oír todavía
en una partida normal: fills y acentos de batería, el bajo que sigue los acentos y toca frases,
melodías por figura, solo de batería de C2, rangos de tempo de los patrones. V4 cerrada el
2026-11-20: el comping escucha los acentos y no se pisa con el bajo (modo Random por semilla),
y el bajo ya no se calla con progresiones más cortas que la parte (MGP-BASS-TILE-1); recetas
listas en el Banco de cartas («📥 Pegar receta»).

Restricciones: logs con la etiqueta [MUSICA-1]; no escribir .meta; no editar MidiGenPlay (lo que
falte en el paquete va a su cola); sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio,
de oído; pasos de editor uno a uno; todo lo que se enciende es opt-in por carta y se puede apagar.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página de
frentes y el arranque de la siguiente sesión del orden de entrada (PUB-SHIELD-1 o S10, según el
calendario).
```

## Qué se pide

1. **D-GF (abierta desde DEV-TUNING-1, 2026-11-17).** El Finisher Gran Final (Zig) toca su solo con soundfont
   encima de la voz (`finisherSoloStrategy = 1`, sin instrumento propio). Dónde mirar:
   `CardDefinition.FinisherSoloStrategy` (`Assets/Scripts/Cards/CardDefinition.cs`), «Solo: estrategia» en el
   `CardEditorWindow`, `ECON2_InspirationSetupTool` y el loop final de solos de ECON-2 (`SSoT_Scoring_and_Meters`,
   Inspiración); la voz de Zig es `SingerVoiceDirector` / V8.
   - A: la voz canta el solo (recomendado).
   - B: dejarlo como está.
   - C: DUET-1, Zig a dos voces, después de la demo.
2. **Encender y oír, carta a carta,** lo que ya está hecho en MidiGenPlay: fills y acentos, el bajo que sigue los
   acentos (y sus frases), melodías por figura; y **V4, el comping que escucha los acentos** (cerrada el 2026-11-20;
   `SongConfigBuilder.ListenSegment` ya incluye el Backing, ALWTTT `d9bd964`; pruebas en el juego 1–8 aprobadas, en
   `MidiGenPlay/Documentation~/research/v4-comping-pruebas/`). Lo que no suene bien se queda apagado. V7 (la melodía
   escucha a la banda) es la siguiente de MidiGenPlay y no entra aquí salvo que se cierre antes.
3. **Pasada por todas las cartas** (pedido de Claudio, 2026-11-17) con el Banco de cartas: escucha, BPM de cada carta y
   rangos de tempo de sus patrones (PATRON-BPM-1 ya está).
4. **Pendientes de oído:** el solo de Sibi con la banda al 55 %.
5. **MGP-ARTIC-SCHED-1 (añadido el 2026-11-20, noche; hecho en MidiGenPlay `654fcf4`, suite en verde).** La
   articulación Random de los acordes sonaba «muy random» incluso a reroll 0,25. La carta de acordes tiene ahora
   `articulationSchedule` (PerEvent por defecto = igual que antes; HoldThenTurn; AlternateBars),
   `articulationPhraseBars` (4) y `articulationAlternateEvery` (1). Falta aquí: (a) fila en el Banco de cartas para
   esos tres campos, junto a Chord Expression; (b) A/B de oído con Wormus Major (y Core Major / Minor): PerEvent a
   0,25 frente a HoldThenTurn N = 4, con la carta de ritmo en `fillEveryNMeasures = 4` para que el giro caiga en el
   fill; (c) de ese A/B salen la confirmación de las D-SCHED-* y si el bajo entra (D-SCHED-BASS, hoy no). Contrato:
   `MidiGenPlay/Documentation~/runtime/SSoT_Composer_Backing_Track.md` §8.5 «Bar-plan schedules».

## Lo que ya existe y hay que reutilizar

- Banco de cartas (`ALWTTT ▸ Banco de cartas`): paridad de bytes con el gig, ↳ de dónde sale cada pista, fila de
  paleta bajo las cartas de ritmo, BPM forzado; desde V4, filas por carta (Accent Follow, Chord Expression,
  Arpeggio Rate, progresión), línea «escucha» con 📋 y «📥 Pegar receta».
- Checklist de las pruebas de V4 en el juego: https://claude.ai/artifact/9Ym2UruWeZpafUeKUQKpJR (recetas reutilizables).
- Pruebas repetibles: semilla fijada y mazo fijo en `GigDevSettings` (`SSoT_Dev_Mode` §20).
- COMP-LOG-1: log de composición en `DevTelemetry/` (§22).
- Cola de MidiGenPlay: `D:\Projects\MidiGenPlay\MidiGenPlay\Documentation~\planning\active\Sesiones_MidiGenPlay.md`
  («Tanda extra»).

## Decisiones previsibles (formato D)

- **D-GF:** arriba.
- **D2 · Qué se enciende para la demo:** todo lo que pase la prueba de oído / solo lo de las cartas de la demo.
- **D3 · Cómo se prueba:** hoja de oído a ciegas A/B (artefacto con db) / escucha en el juego.

## Smokes (≤3, jugados por Claudio, de oído)

1. El Gran Final con D-GF aplicada: la voz y el solo no se pisan.
2. Una canción con fills, acentos y el bajo encendidos: la banda suena a banda que se escucha, sin cortes.
3. Regresión: con todo apagado, la misma semilla suena igual que antes (byte a byte en el Banco).

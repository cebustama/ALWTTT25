# Arranque MUSICA-1b · encender lo de MidiGenPlay carta a carta

Escrito el 2026-11-20. Planning, no autoridad. Continuación de MUSICA-1 (cerrada parcial el 2026-11-20). Va
**después de S10**; si no cabe antes del 27-nov, pasa a POST-DEMO B10.

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 MUSICA-1b — la banda que se escucha, carta a carta.

Lee Docs/planning/active/ARRANQUE_MUSICA-1b.md. Antes, comprueba el calendario (S10 y S11 tienen
prioridad antes del 27-nov). D-GF ya está hecha (MUSICA-1); las decisiones D2 = A, D3 = A y
D4 = A siguen vigentes.

Nuevo desde MidiGenPlay (654fcf4, suite en verde): MGP-ARTIC-SCHED-1. BackingCardConfigSO tiene
articulationSchedule (PerEvent por defecto = igual que antes; HoldThenTurn: figura A los N−1
compases y B en el último de cada frase; AlternateBars: ABAB/AABB), articulationPhraseBars (4)
y articulationAlternateEvery (1). Solo actúa con chordExpression = Random. Contrato: MidiGenPlay
Documentation~/runtime/SSoT_Composer_Backing_Track.md §8.5 «Bar-plan schedules».

Pide, en este orden: fila en el Banco de cartas para los tres campos nuevos; A/B de oído con
Wormus Major (Core Major/Minor): PerEvent a 0,25 frente a HoldThenTurn N = 4, con
fillEveryNMeasures = 4 en Default Mode para que el giro caiga en el fill (de ahí salen las
D-SCHED-* y si el bajo entra, D-SCHED-BASS); encender carta a carta (tabla de abajo); pasada
por el mazo de la demo (BPM y rangos de tempo).

Restricciones: logs con la etiqueta [MUSICA-1]; no escribir .meta; no cambiar el comportamiento
del paquete desde ALWTTT; decisiones en formato D; ≤3 smokes jugados por Claudio, de oído; pasos
de editor uno a uno; sin commit ni push sin pedirlo.
```

## Encender (D2 = A, una carta por pista; el resto queda de control)

| Pista | Carta (bundle) | Qué se enciende | Control |
| --- | --- | --- | --- |
| Ritmo | Default Mode (`starter_default_mode_Payload_Rhythm_StyleBundle`, única de C2) | `fillEveryNMeasures` 4, `crashAfterFill`, `fillPalette` = `Assets/MidiGenPlayFills/FillLibrary` | — |
| Bajo | Slap Groove (`BasslineCardConfig_SlapGroove`) | Accent Follow = Phrases, estilo Funk, sigue la caja, notas muertas | Walking Bass |
| Acordes | Wormus Major (`Backing Card Config - Core Major`, compartido con 3 cartas viejas fuera de la demo) | AttacksAndGaps, umbral 0,6, carriles = caja, entrada y cierre del fill | Wormus Minor |
| Melodía | Rise Up (`MelodyCardConfig_RiseUp`) | Modo figura con los ajustes de `TEST_MelodyFigure_StyleBundle` | Showtime |

Recetas aprobadas: pruebas V4 en el juego (MidiGenPlay `research/v4-comping-pruebas/resultados-juego-2026-11-20.md`,
checklist https://claude.ai/artifact/9Ym2UruWeZpafUeKUQKpJR).

## Smokes (≤3, de oído)

1. Una canción con las cuatro cartas encendidas: banda que se escucha, sin cortes; el giro de articulación cae en el fill.
2. A/B de `articulationSchedule` en el Banco (PerEvent 0,25 frente a HoldThenTurn N = 4).
3. Regresión: con todo apagado, la misma semilla suena igual (byte a byte en el Banco).

## Pendiente fuera de esta sesión

- El 2.º solista armoniza al 1.º (POST-DEMO B9, con DUET-1).

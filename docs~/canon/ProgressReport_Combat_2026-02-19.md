# ALWTTT — Combat Pipeline — Progress Report (2026-02-19)

## Qué se validó en Play Mode (estado actual)
### Deck/Hand
- Al iniciar el Gig, el mazo se setea correctamente y se **roban 5 cartas**.
- Las cartas **aparecen en mano**, se pueden arrastrar y jugar.

### Ejecución de cartas (Effects-first)
- `CardBase.Use()` → `ExecuteEffects()` se dispara al soltar la carta en la zona del Gig.
- Efectos confirmados como OK:
  - **ModifyVibe**: cambia Vibe en audiencia (visible + logs).
  - **ModifyStress** (positivo/negativo): cambia Stress en músico (visible + logs).
  - **ApplyStatusEffect**: aplica stacks de status (logs confirman).

## Qué NO tiene efecto visible todavía (gaps)
### Composure
- El status se aplica (stacks), pero **no está absorbiendo Stress entrante**.
- DoD esperado: Stress positivo debería consumir Composure primero, luego aplicar el remanente.

### Flow
- El status se aplica (stacks), pero **solo se “ve” en el cálculo de SongHype por loop**.
- En la corrida actual hay warnings de MPTK (“No MIDI found in MIDI DB”), lo que sugiere que los loops no están completando → Flow no puede manifestarse.

## Observaciones útiles
- El log muestra: **Initial deck resolved: Action=5, Composition=0**.
  - Para la slice de Action Cards está bien, pero si quieres mano mixta, falta inyectar composición.

## Próximos pasos inmediatos (orden recomendado)
1) Implementar **Composure absorption** en el path de aplicación de Stress (solo para Stress positivo).
2) Hacer **Flow testeable** sin depender de MIDI:
   - opción A: agregar 1 MIDI a la DB (MPTK), o
   - opción B: agregar un “DebugSimulateLoopEnd()” que dispare el mismo cálculo de loop-end.
3) Agregar logs/readouts mínimos:
   - Stress actual
   - Composure stacks
   - Flow stacks (total banda)
   - ΔSongHype por loop (con y sin Flow)

## Checklist de validación (MVP)
- [x] Start gig → se roba mano (cartas visibles)
- [x] Add Vibe → Vibe cambia
- [x] Add/Remove Stress → Stress cambia
- [x] Add Flow → stacks se aplican
- [x] Add Composure → stacks se aplican
- [ ] Composure absorbe Stress (pendiente)
- [ ] Flow aumenta ΔSongHype por loop (pendiente de loop-end)

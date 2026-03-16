# SSoT_Combat — Patch/Addendum (2026-02-19)

Este archivo documenta el estado de implementación vs el contrato del SSoT (solo delta).

## Implementado
- Deck/Hand pipeline operativo en play mode.
- Execution de cards (effects-first) funcionando:
  - ModifyVibe OK
  - ModifyStress OK
  - ApplyStatusEffect (stacks) OK

## No implementado / no validado aún
### Composure absorption
- Falta interceptar Stress positivo para consumir Composure antes de aplicar Stress.
- DoD: logs de absorbed/remainder + valores correctos.

### Flow multiplier en loop scoring
- Falta evidencia (o trigger) de loop-end scoring ejecutándose en play mode.
- Solución MVP: MIDI DB mínima o debug loop-end.

## Riesgos / notas
- Warnings de MPTK “No MIDI found…” bloquean validación de mecánicas dependientes de loops.
- El test actual muestra Composition=0; si se busca mano mixta, falta inyectar composición en el mazo inicial.

# Roadmap_Combat — Patch/Addendum (2026-02-19)

Este archivo es un **addendum** (parche) para el roadmap del Combat MVP.

## Qué se completó desde la última sesión
- ✅ La pipeline Deck/Hand está estable: SetupDeck → DrawCards → mano visible.
- ✅ Se puede jugar cartas y ejecutar efectos end-to-end (drop zone → CardBase.Use → effects).
- ✅ ModifyVibe y ModifyStress funcionan.
- ✅ ApplyStatusEffect aplica stacks.

## Qué falta para cerrar el MVP slice
### 1) Composure absorption
- Implementar consumo de stacks antes de aplicar Stress positivo.

### 2) Flow hook en loop scoring (validación)
- Flow debe aumentar el ΔSongHype por loop cuando FlowStacks > 0.
- Actualmente no se observa por falta de loops (MIDI DB vacía) → agregar MIDI o debug loop-end.

## Checklist actualizado
- [x] Robar mano al iniciar el Gig
- [x] Jugar AddVibe → Vibe cambia
- [x] Jugar ±Stress → Stress cambia
- [x] Jugar AddFlow → stacks aplican
- [x] Jugar AddComposure → stacks aplican
- [ ] AddComposure + AddStress → Composure absorbe primero
- [ ] Flow > 0 → ΔSongHype por loop aumenta

## Próxima tarea recomendada (1er paso)
Implementar `ApplyIncomingStressWithComposure(...)` o equivalente y enrutar TODA subida de Stress por ahí.

# Rehydration Prompt — ALWTTT Combat MVP (Deck/Hand + Action Cards) — 2026-02-19

Estamos continuando el trabajo en **ALWTTT**, enfocándonos en la slice MVP:

**Deck/Hand → Play Card → Execute Effects → Status/Stats → Loop scoring hooks (Flow) → Stress mitigation (Composure)**

## Estado actual (confirmado)
- La mano se roba correctamente al iniciar el Gig.
- Las cartas son visibles y jugables.
- Efectos funcionando: ModifyVibe, ModifyStress, ApplyStatusEffect (stacks aplican).
- Problema pendiente: Flow y Composure no tienen efecto observable aún (por triggers faltantes).

## Objetivo inmediato
1) Implementar **Composure absorption** (consume stacks antes de aplicar Stress positivo).
2) Validar **Flow** afectando el cálculo de SongHype por loop:
   - activar loops reales (MIDI DB) o
   - agregar un “debug loop-end” que ejecute el scoring una vez por botón/tecla.

## Definición de Done de esta iteración
- Al jugar Composure y luego aplicar Stress +N:
  - Composure baja primero, Stress sube solo por el remanente.
  - Logs muestran absorbed/remainder.
- Al tener Flow > 0:
  - el ΔSongHype por loop aumenta (logs muestran multiplicador).

## Archivos sugeridos para adjuntar (máx. 10)
1) GigManager.cs
2) DeckManager.cs
3) HandController.cs
4) GameManager.cs
5) CardBase.cs
6) ApplyStatusEffectSpec.cs
7) ModifyStressSpec.cs (o donde se ejecuta ModifyStress)
8) StatusEffectContainer.cs
9) StatusEffectSO.cs + StatusEffectCatalogueSO.cs (si cuentan como uno, adjuntar ambos)
10) SSoT_Combat.md y/o Gig_Combat.md (si solo cabe uno: SSoT)

## Notas de scope
- No expandir song generation internals.
- Solo lo necesario para que Flow/Composure se validen en play mode.

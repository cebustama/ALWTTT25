# ALWTTT Documentation — Index

Esta carpeta está pensada como una **mini‑wiki** del proyecto.
La documentación se organiza alrededor de un **canon mínimo de 3 docs**,
y el resto queda como reference/backlog/archive.

---

## Canon (source of truth)

1) **SSoT — Combat** (cómo funciona el pipeline end‑to‑end)
- `canon/SSoT_Combat.md`

2) **Roadmap — Combat** (qué falta implementar + DoD)
- `canon/Roadmap_Combat.md`

3) **Appendix — Authoring & Data Contracts** (EditorWindow + JSON + status identity; no‑gameplay)
- `canon/Appendix_Authoring_DataContracts.md`

> Regla: si un doc legacy contradice un doc en `canon/`, **gana canon**.

---

## Reference (páginas “wiki” estables)

- Gig (encounter): `reference/Gig.md`
- Gig Combat (economy contract + addendum táctico): `reference/Gig_Combat.md`
- Cards (modelo + runtime): `reference/Card.md`
- Status effects (sistema + catálogo): `reference/StatusEffects.md`
- CSO primitives (catálogo + referencias): `reference/StatusEffects_Primitives_with_References.md`
- Audience member (modelo/rol): `reference/AudienceMember.md`

### Subsystems
- MIDI Gen Play: `reference/subsystems/MidiGenPlay/`

---

## Backlog (no normativo)

Ideas, expansiones y mecánicas futuras (útiles, pero no obligatorias).

- Triggered mechanics + extensions: `backlog/CardEffects_Extensions_Backlog_and_TriggeredMechanics.md`
- Notas sueltas: `backlog/ideas/`

---

## Archive (legacy / histórico)

Material conservado por contexto; **no** es autoridad.

- `archive/`  
  - Recomendación: mantener aquí snapshots y “docs reemplazados”.

Banner recomendado para docs legacy:
- `STATUS: LEGACY`
- `Replaced by: canon/<...>`

---

## Convenciones rápidas

### Niveles de autoridad
- **CANON:** `canon/` (normativo)
- **REFERENCE:** `reference/` (informativo/estable)
- **BACKLOG:** `backlog/` (ideas)
- **LEGACY:** `archive/` (histórico)

### Cómo versionar sin explotar la carpeta
- Canon mantiene **nombres estables**.
- Si quieres snapshots: copia la versión anterior a `archive/snapshots/YYYY-MM-DD_<name>.md`.


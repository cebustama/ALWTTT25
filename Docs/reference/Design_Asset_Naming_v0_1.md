# Design_Asset_Naming_v0_1 — ALWTTT composition asset naming convention

**Status:** planning — **non-normative**. This document does not override any SSoT and is
not implementation authority.
**Created:** 2026-07-20 (CSV-4, closing worklist item E; D-CSV-7=A).
**Placement:** `planning/` root — this is a standing convention, not a batch artifact.
**Application batch:** **CSV-4b** (drafted here, applied there — D-CSV-19=A).
**Arc home:** `planning/active/CSV_Composition_Validation_Sub_Roadmap.md`.

---

## 1. Scope

This convention applies to **`Assets/` only.**

Package assets under `Packages/com.claudiobustamante.midigenplay/` are named by
MidiGenPlay. Under **D-CSV-7=A**, asset ownership is location-based: `Assets/` is
ALWTTT's, `Packages/` is MidiGenPlay's. Anything ALWTTT wants renamed package-side is an
**ask**, filed per `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8 — never an edit, because edits
under `Packages/` revert on package update.

Out of scope for v1: style bundles (§7), and every asset family not listed in §3.

---

## 2. Rider — naming authority is not moving authority

**A rename must not change an asset's position relative to a Resources scan root.**

- In-place renames — file and folder — are fine.
- Relocations across scan roots are **D-CSV-14**, owned by **CSV-5**, and are a separate
  decision with runtime consequences (`OFF-ROOT` resolution).

The two operations look similar in an editor tool and are not similar in effect. A
renamer window (CSV-4b) must not offer "move" as a convenience.

---

## 3. Schema per family

| Family | Convention | Worked example |
| --- | --- | --- |
| Chord progression | `Prog_<TS>_<Nm>_<RomanSlug>[_<Mode>]` | `As` → `Prog_4-4_8m_As` |
| Drum pattern | `Drum_<TS>_<Nm>_<Style>` | `WL_JazzWaltz` → `Drum_3-4_2m_JazzWaltz` |
| Melody pattern | `Melody_<TS>_<Nm>_<Nn>` | `Melody_6-4_2m_11n` → `Melody_6-8_2m_11n` |
| Palette | `Palette_<Family>_<Scope>` | `DrumPatternPalette-WaltzLilt` → `Palette_Drums_WaltzLilt` |
| Folder | meter-named, no spaces | `Patterns/Drums/New folder` → `Patterns/Drums/Mixed` |

Token definitions:

- `<TS>` — time signature, hyphenated: `4-4`, `3-4`, `6-8`, `5-4`.
- `<Nm>` — measure count with a trailing `m`: `8m`.
- `<Nn>` — note count with a trailing `n`, melody only: `14n`.
- `<RomanSlug>` / `<Style>` / `<Scope>` — short human token, no spaces.
- `<Mode>` — optional modal qualifier where the progression is not plainly
  major/minor (`Dorian`, `Aeolian`, …). Note that CR-10 / D-CSV-23 dissolves the Modal
  *palette* at CSV-6; the modal qualifier on an individual asset name survives that
  change and is how the colour stays legible once the palettes are merged.

The meter appears in the name because the meter is the one property that determines
whether an asset is usable at all in a given part, and it does not change.

**Patrones de percusión (aplicado CONT-B, 2026-07-31).** Prefijo de paleta +
nombre + sufijo de longitud: `FF_Rock8c`, `SP_Funk8c`, `FF_LatinSon32_8c`.
Prefijos en uso: `FF_` (FourOnTheFloor) · `SP_` (SyncopatedPocket) · `WL_`
(WaltzLilt) · `OM_` (OddMeterAngular) · `CS_` (CompoundSwing). El sufijo `8c`
distingue el estándar de 8 compases del material heredado de 2. El prefijo
declara la paleta de destino, no la ubicación en disco — que es
`Patterns/Drums`, la raíz de escaneo (ver D-CSV-14).

> **Reconciliation owed:** the applied CONT-B drum naming (palette prefix + `8c` suffix) diverges from this doc's draft `Drum_<TS>_<Nm>_<Style>` schema and from §4 ("why the prefixes go"). The draft was never applied (CSV-4b); reconcile the schema with the applied reality at CSV-4b.

---

## 4. Why the `WL_` / `FF_` / `OM_` / `CS_` prefixes go

These prefixes encode the **palette** an asset was authored for. That breaks the moment a
pattern is reused in a second palette — and reuse is the normal case, not the exception.
A name that lies after its first reuse is worse than a name that says less.

The meter does not change. The palette does. Name for the invariant.

---

## 5. Why the `New folder` rename is `Mixed`, not a meter

Its five patterns span 3/4, 4/4, 5/4 and 6/8. No single meter folder fits, and splitting
them across four folders for tidiness would be a **relocation**, which §2 forbids at this
batch. `Mixed` is honest and is a one-step in-place rename.

---

## 6. Human-readable strings move to `displayName`

Several assets already populate `displayName`. Where a machine-shaped asset name loses
information a human wanted (`I (1) – IV (1) – V (1) – I (1)`), that string moves to
`displayName` rather than being discarded.

**The UI must not regress when asset names become machine-shaped.** Any surface that
currently renders the asset name and would degrade should be switched to `displayName`
with an asset-name fallback, and that check is part of CSV-4b's acceptance, not an
afterthought.

---

## 7. Style bundles are out of scope for v1

Bundle names matching `*_Payload_<Role>_StyleBundle` are **tool-generated**.
`CardAssetFactory` was checked and is **not** the generator; the emitting authoring path
is currently **unidentified**.

Two consequences:

1. Hand-renaming them is wasted work — the next authored card reintroduces the pattern.
2. Renaming them may break an assumption in whatever path does emit them.

**Identify the generator before including bundles in this convention.** Until then,
bundle names are left alone.

---

## 8. Safety note

`AssetDatabase.RenameAsset` preserves GUIDs, so **references survive a rename by
construction** — a renamer window does not risk the reference graph.

The residual risk is **name-based lookup**: `Resources.Load("Name")`, string comparison
against an asset name, or any serialized string that duplicates a name. Audit for this
**once**, before any bulk run. It is a cheap grep and an expensive surprise.

---

## 9. What this document does not do

- It does not rename anything. Application is **CSV-4b**.
- It does not authorise moving assets between Resources roots (§2).
- It does not cover package assets (§1).
- It does not override `SSoT_Editor_Authoring_Tools.md` §17, which owns the inventory
  window's surface, including the read-only invariant that CSV-4b must preserve.

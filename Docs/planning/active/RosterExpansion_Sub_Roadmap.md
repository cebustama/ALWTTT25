# RosterExpansion_Sub_Roadmap — ALWTTT

**Status:** Planning-only. Decomposes `Roadmap_ALWTTT.md` → Future Milestones → *Roster Expansion* into an executable campaign (R0–R8). Does **not** define implementation truth. **R0 CLOSED 2026-07-23** (design record: `planning/active/Design_Starter_Deck_v2.md`); next batch **R1**, interleavable with S5i.
**Pattern:** Same role as `S5_DemoCutClose_Sub_Roadmap.md` / `M1_5_Dev_Mode_Sub_Roadmap.md` — a durable multi-batch plan + decision ledger; per-batch **deep scope + rehydration prompt are generated at batch-open**, not duplicated here.
**Classification:** `roadmap` (planning-only) — not a SSoT.
**Created:** 2026-07-23 (feasibility + planning session). Per D6=A this document is the single consolidation home for that session; the detailed per-card reasoning lives in the session record and in the thematic design notes it points to.
**Placement:** `Docs/planning/active/RosterExpansion_Sub_Roadmap.md`

---

## 0. Position and phasing (D1=C)

The campaign redesigns the starter deck to cover the **4 band musicians** (Sibi, C2, Conito, Zig) and populates the deferred **finisher layer** (D-ECON-6=DEFER, `SSoT_Gig_Combat_Core.md §14.6`). It became executable because both hard prerequisites of the original Roster Expansion entry resolved:

- **Bass pipeline — validated.** BASS-1 + BASS-CARD-1 (2026-07-12): tracks keyed `(musicianId, role)`, `BasslineCardConfigSO`, role-typed `styleBundleCreate` authoring.
- **`ApplyIncomingVibe` — shipped** (2026-05-18, B3) and already the canonical path for all card-sourced positive Vibe (`CardBase.cs`) and Earworm ticks. **Captivated — shipped R1** (2026-07-23) as an amplification layer inside that same helper; no `CardBase` or `GigManager` change was needed. Both Roster Expansion prerequisites are now closed.
- **Singer voice — live** (SINGER-1, 2026-07-21). `SSoT_Singer_Voice.md §8` already names *"Zig's self-harmony finisher"* as the intended first consumer of voice slot 2.

**Phasing rule (D1=C).** The live demo front stays **S5i → S5j** untouched.

- **Interleavable with S5i:** R0 (design, no code) and R1–R3 (enablers). Interleaving-safety argument: new cards live in the **Conito / Cantante catalogs**, which are out of the demo roster and therefore excluded *by construction* from `BuildRewardCardPool` (band-scoped, `PersistentGameplayData`); the Captivated amplification layer is inert without a sender in the demo roster. The S5i tuning baseline (17-card / 2-musician starter) is not perturbed.
- **Post-S5j only:** R4+ — anything touching the starter deck, the finisher layer, the tutorial script (Psychic Waves is the guided finisher, TUT-REBUILD beat 8), or session runtime invariants.

---

## 1. Campaign requirements (spec, 2026-07-23)

Per musician (×4):

- **Starter identity:** 2 distinctive composition cards + 1 distinctive action card + 1 finisher card with a per-musician unique mechanic (finisher = `inspirationCost > 0`, per §14.6).
- **Rewards:** 2 "path" composition cards (two different musical directions per musician; **soft paths per D5=A** — both coexist in the flat per-musician reward pool, no exclusivity mechanic) + 1 action card carrying a status associated with the musician.

Plus one new cross-cutting mechanic in-campaign (**Track Card Levels**, §7 / batch R7) and two registered-only ideas out of campaign (§7).

---

## 2. Decision ledger (campaign-level)

Per-batch decisions get their own `D-RX-*` labels at batch open; these are the campaign-level ones.

- **D1 = C** — Hybrid phasing: R0 + non-demo enablers (R1–R3) may interleave with S5i; starter-v2 / finisher / tutorial-touching batches (R4+) open only after the S5j tag. **Amended 2026-07-31 (D-SEQ-3=A):** R4+ open after the **snapshot tag**, not the demo-cut (S5j) tag.
- **D2 = A** — Reuse the existing card baseline where it already satisfies the spec (Wormus ×2 pair, Default Mode, Keep Cool, Psychic Waves-extended, Waltz Protocol, reward-pool cards); author only the gaps. No from-scratch re-author.
- **D3 = A** — Conito bass ships **v1 approximations** now (root-note bass + articulation figures + slap/nylon patches); the fidelity items (chord-tone walk, pocket-coupling, bossa split) are **MidiGenPlay asks**, not ALWTTT work (§8).
- **D4 = A** — Double Harmony ships **Tier A** (MIDI Harmony-role track; composer exists package-side) in-campaign; **Tier B** (second sung voice, slot 2) is a follow-up gated on the deferred cap=2 Dev Mode validation and the ~21% two-voice DSP budget.
- **D5 = A** — Reward "paths" are **soft**: both direction cards coexist in the per-musician pool (`RewardPool ∩ UnlockedByDefault`, owned excluded per D9). Exclusive branching is a Progression & Meta mechanic, retrofittable.
- **D6 = A** — Documentation packaging: this sub-roadmap is the single consolidation home (compact verdict table §5); no separate feasibility-eval document. Thematic design notes exist only for content that feeds batches or registers ideas (§7).
### R0 decision ledger (locked 2026-07-23)

Full reasoning + card specs: `planning/active/Design_Starter_Deck_v2.md`.

- **D-R0-1 = A** — Sibi taste-reveal card = **Read the Room**, RewardPool, authored at **R4** (a Sibi-catalog pool card would perturb the demo reward pool, so it cannot interleave with S5i).
- **D-R0-2 = B (amended)** — **Compound Cycle (6/8) promoted to starter** as C2's second composition; **Waltz Protocol (3/4) moved to reward pool**. 1:1 swap, starter size unchanged. Inverts the original D-STARTER-2=B placement; meter-axis run-growth headroom now lives in Waltz + Pentameter.
- **D-R0-3 = A** — Keep Cool retargeted `Self` → `Musician`. Tutorial-beat regression duty at R4.
- **D-R0-4 = A** — Conito draw card = **Static Rush**: `DrawCards(2)` + `ModifyStress(+1, Self)`.
- **D-R0-5 = A + rider** — **Overload is Action-domain** (playable on the final loop, where inv 11 denies only composition plays). **Rider:** Voltage is a standard SO-catalogue counter status, so explicit card-driven Voltage generation is supported today via `ApplyStatusEffectSpec` (Amp Up = zero new spec code); R5's only new runtime is the per-play passive hook + the Overload threshold/consumer.
- **D-R0-6 = B** — Starter v2 = **22 cards / 18 unique** (16 musician-owned + 2 generics), symmetric kit shape 2 comp + 1 action + 1 finisher per musician. Push It, Half Time, Key Lift, Singing Field move to the reward pool.
- **D-R0-7 = locked bundle** — Levels: per-part lifetime · max 3 · level discarded on replace-by-different-card · level-up is a normal composition play · badge + floater UI · no Action-card levels · Wormus-only pilot. Home: `Design_Track_Card_Levels_v0_1.md` §7 (v0.2).
- **D-R0-8 = A** — +INSP-per-level **reserved**; complexity term remains the intended hook, filed to its S5i owner.
- **D-R0-9 = locked** — Singalong = `ApplyStatusEffect(captivated, +1, AllAudienceCharacters)`, one-shot phrase + choir echo, no phase gate, three-rung degradation ladder.
- **D-R0-10 = A** — Zig reward directions = **Torch Song** (ballad, slow-lean) / **Motor Mouth** (patter, fast-lean).
- **D-R0-11 = A** — Existing pool cards (Vamp, In the Pocket, Pentameter, and the starter→pool moves) are **extras**, not reward-slate slots.
- **D-R0-12 = agreed** — Finisher cost band first guess: Psychic Wave v2 = 3 · Spotlight = 2 · Overload = 2 (+ Voltage ≥ 3) · Double Harmony = 3. Tuned at R8 (N5).

### R2 / R2c / R2d decision ledger (locked 2026-07-31)

- **R2 / R2c / R2d batch ledger.** D-R2-1=A (Static Rush's +1 Stress routes through Composure
  absorption unchanged) · D-R2-2=A (Finger = on-beat, Slap = Offbeat; contrast axis is rhythmic
  displacement) · **D-R2-3 = `RandomFromList` over {Slap Bass 1, Slap Bass 2}** (revised
  2026-07-31 from "Slap Bass 1 fixed, provisional": the new mode makes both patches live, which
  also retires the pending re-listen) · D-R2-4=A (author the real chord-tone walk now) ·
  D-R2-5=A (close R2 with the package's bass-register defect as a known state rather than
  waiting for MidiGenPlay's content phase B; **materially mitigated** by the SelfPocket +12 pop
  contour) · D-R2-6=B (wire SOLO-1 into the gig path, sourced from a **palette**) · D-R2-7 (new
  `InstrumentEffect.RandomFromList`; pick once per application, persist as a specific override) ·
  D-R2-8=B (**the Conito catalog cleanup removed the 10 test entries rather than flattening
  their flags to `None`** — diverges from the R1 precedent D-R1-4=A; accepted because
  reversibility is equivalent and the hard constraint "no assets deleted" held. Recorded, not
  silently normalized) · D-R2-9=A (fix order-dependence via a package ask with full precedence
  semantics, not by reordering ALWTTT's track list — list order is consumer identity) ·
  D-R2-10=A (shared-harmony cache identity from **pre-render** segments `dp:` + `bk:`; the
  package readback cannot serve as a key) · D-R2-11 (adopt `SelfPocket` for Slap Bass v1).

### R4 decision ledger (recorded at close, 2026-08-10)

- **D-R4-1** — heredada de R3, **sigue abierta**. Dirección decidida (el público debe juzgar lo
  que suena — RECONOCER); **cableado PROHIBIDO** (NO CONSUMIR). Lote propio, antes de R8.
- **D-R4-2 = defer-A** — calificador de canción no se cablea; A como dirección, C como destino,
  B solo override.
- **D-R4-3 = A** — `RedirectIncoming = 504` añadido al CSO (rango Control; 404 lo ocupa
  `NegateIncomingPositive`, sin colisión).
- **D-R4-4 = A** — panel de gustos persistente en el canvas (revisado por D-R4-10).
- **D-R4-5 = A** — `PsychicWaveOverlayController` dedicado, escucha el bus.
- **D-R4-6 = A** — registro CSO repoblado desde canónico tras añadir el `case`.
- **D-R4-7** — abierta (VFX v3 Psychic Wave: doble onda + inversión de color, shader propio).
- **D-R4-8** — abierta (legibilidad del taunt; recomendación A = floater en el objetivo original).
- **D-R4-9** — verificada empíricamente (`Tonality`/`TimeSignature` renderizan legibles; sin
  formatter — pero ver F-R4-2).
- **D-R4-10** — **cerrada en PRES-1 (2026-08-11)**: reveal compuesto en el tooltip de hover +
  icono persistente discreto; panel retirado.
- **D-R4-8** — **cerrada en PRES-1 (2026-08-11)**: floater de redirect en ambas ramas, con
  supresión logueada del no-op visual.

### R5 decision ledger (apertura, 2026-08-11)

- **D-R5-1 = A** — el doc-update de PRES-1 se aplica como **fase de apertura de R5**
  (`DOC-APPLY-PRES1`), antes de tocar código. Motivo: R5 y F-PRES1b-1 escriben en los mismos
  documentos y las mismas secciones que PRES-1 dejó pendientes; apilar un segundo diff sobre uno
  sin aplicar deja dos futuros compitiendo por la misma línea.
- **D-R5-2 = A (resuelve F-PRES1b-1)** — el objetivo por defecto de una acción mono-objetivo de
  audiencia es el músico **más cercano al Breakdown**: `CurrentStress` absoluto más bajo bajo el
  medidor invertido S5e. El comparador `>` sobrevivió a la inversión S5e sin voltearse porque el
  selector lee el campo crudo, fuera de la API direction-agnostic que S5e sí protegió; regresión
  **inferida**, no diseño. **Absoluto y no proporcional**: el Breakdown dispara a 0 absoluto, así
  que los puntos restantes son la distancia mecánica al colapso — una regla proporcional a veces
  pasaría por alto al que está más cerca de caer. Cambio de GAMEPLAY: ST-R5pre-1..4 + regresión
  ST-PRES1-4/-6.
- **D-R5-3 = A** — `StatusEffectWizardWindow` gana campo `statusKey` editable pre-rellenado y
  escritura explícita en `CreateAssetAndRegister` (cierra **F-R4-3**), más guard de clave
  duplicada. Rider en el mismo toque: el auto-find solo auto-asigna catálogo si hay exactamente
  uno. Se valida solo al crear el estado Voltage del propio R5.
- La **review de invariantes de sesión** exigida por la fila R5 (§3 + nota de §5) es la fase
  siguiente (R5-inv) y precede a cualquier código del núcleo de Overload. Punto de choque ya
  identificado: el revert one-loop contra la persistencia actual del part cache.

### R5 decision ledger — fases R5-inv / R5-a / R5-b / R5-c (aplicado 2026-08-23, DOC-APPLY-R5)

**R5-inv (2026-08-11)** — siete invariantes clasificados contra código. Dos correcciones al
plan: el patrón economy-neutral D-CSV-24 **no es reutilizable** (`_devInjectedTrackKeys` es
`#if ALWTTT_DEV`, no existe en producción) y no hace falta — un track de solo con
`inspirationGenerated = 0` y sin `AddInspirationPerLoopSpec` contribuye 0 a `EvalPerLoopInsp`
por construcción; y el precedente real de alcance-un-loop es **JAM-1/JAM-2**, no D-CSV-24.
**Verificado además:** el turno de audiencia se dispara solo en boundaries de **canción**
(`GigPhase.AudienceTurn` se fija en `SongPerformanceRoutine` y en `OnCompositionSessionEnded`),
nunca en `OnCompositionLoopFinished` ⇒ un loop de bonus **no** concedería turno de audiencia
extra ni decaería estados de audiencia una vez más.

- **D-R5-4 = A** — el solo de un loop se inyecta con **alcance de render** en el `SongConfig`
  (patrón JAM-1/JAM-2), no como mutación del modelo con revert. Se rechazó (B) añadir API de
  borrado al modelo — abriría reversibilidad para todas las cartas — y (C) "no revertir", que
  con parts-per-song = 1 equivale a "hasta el fin de la canción" y explota en cuanto haya
  canciones multi-parte. **Coste aceptado:** el solo no se ve como fila de track; su legibilidad
  es presentacional. **Forma confirmada por el usuario:** la base sigue sonando; Conito toca el
  solo **encima**, no en sustitución. ⚠ **Decisión vigente, alcance NO construido** — pasa a
  R5-d (ver la nota de sustitución al final de este ledger).
- **D-R5-5 = A** — el hook pasivo "+1 Voltage por jugada de Conito" vive en la rama consumada de
  `GigManager.TryConsumePlay`, único punto por el que pasan las dos rutas de juego con el
  intérprete ya resuelto. Se rechazó (B) duplicar por dominio y (C) suscribirse a
  `CardPlayedEvent`, que **no lleva el intérprete**. **Rider:** colgar de "consumió de verdad",
  no del retorno: con `musician == null` la API devuelve `true` sin consumir. **Asimetría
  heredada de ECON-1, registrada:** la ruta de acción consume **antes** de `heldCard.Use(...)`;
  si alguna vez algo posterior a ese punto puede fallar, Voltage se cargaría en una jugada
  abortada. **CONSTRUIDA en R5-b.**
- **D-R5-6 = B** — el loop de bonus **no** refilla el presupuesto ECON-1 de la banda. Overload da
  música, no tempo de cartas. El resto del boundary (draw F-3, inspiración, bus, Vibe) se
  comporta igual que en cualquier loop. ⚠ **Decisión vigente, alcance NO construido** — no hay
  loop de bonus; pasa a R5-d.
- **D-R5-7 = A** — Voltage recibe primitiva propia **`ResourceCounter = 993`** (rango Meta
  990–1099, libre verificado contra el enum refrescado 2026-08-11 y contra los `case` del
  registro). Se rechazó reutilizar `ResourceGenerationModifier = 992`: semánticamente Voltage
  **es** el recurso, no un modificador de su generación, y como el contenedor está keyed por
  primitiva, la reutilización condenaría a colisión a cualquier futuro estado que sí modifique
  generación. Tras R5-a el registro CSO pasa de 26 a **27** entradas. **CONSTRUIDA en R5-a.**
- **D-R5-8 = A** — Voltage es de **alcance gig**. `ResetSongScopedStatuses` es una allowlist de
  dos primitivas y **no se amplía**. Coste aceptado: banca de cargas de Overload (`MaxStacks 9`).
  Verificado por ST-R5a-6R. Reversión posible, pero exige mover el reset a un cierre de canción
  real. **CONSTRUIDA en R5-a.**
- **D-R5-9 = A** — generación limitada a **Conito** por identidad de músico
  (`MusicianCharacterData.CharacterType`). Sin marcador autorable. Generalizar es un cambio de
  una línea en un solo seam. **CONSTRUIDA en R5-b.**
- **D-R5-10 = A** — cuentan **todas** las jugadas genuinamente consumidas: acción y composición,
  cualquier coste de inspiración **incluido 0**. Bajo D-ECON-6=DEFER todas las cartas starter son
  coste 0; excluirlas habría hecho Voltage inalcanzable con el contenido actual. El rider de
  subir las cartas dev a coste 1 queda anulado. **CONSTRUIDA en R5-b.**
- **D-R5-11 = <pendiente>** — cuándo importar *Amp Up* (R8). Recomendación emitida: **B**
  (importar con `flags = UnlockedByDefault`, sin `RewardPool`, y activar el flag en R8). **No
  resuelta**; la carta no se autoró.
- **D-R5-12 = A** — el interruptor de generación vive en `GigFlowSettingsSO`, no en
  `GigDevSettingsSO` ni en `GigManager`. Default **ON**, leído **por jugada** ⇒ conmutable en
  caliente durante Play. Es una regla de gig, no un debug. Verificado en ST-R5b-7R.
  **CONSTRUIDA en R5-b.**
- **D-R5-13 = A** — Overload multiplica la contribución del loop a **SongHype**. **CONSTRUIDA en
  R5-c.** ⚠ Ver nota de sustitución.
- **D-R5-14 = A** — umbral **6**, consume **6**, autorables. Fijado contra el dato medido en R5-b
  (+2/periodo ⇒ ~3 periodos por carga). Bajo `MaxStacks 9` no hay banca de dos cargas.
  **CONSTRUIDA en R5-c.** ⚠ Contradice `D-R0-12` (Overload = Voltage ≥ 3) — ver nota de
  sustitución.
- **D-R5-15 = A** — disparo **automático** al cruzar el umbral. Coste aceptado: el jugador
  **observa** Overload, no lo decide. **CONSTRUIDA en R5-c.** ⚠ Contradice `D-R0-5=A` (Overload
  es Action-domain, carta jugable) — ver nota de sustitución.
- **D-R5-16 = A** — consumidor en `GigManager`, al inicio de `OnCompositionLoopFinished`,
  **antes** de `TriggerAudienceMicroReactions`. Reset-primero: el factor no sobrevive al loop. No
  se crea `CardEffectSpec` nuevo: sin carta, no hay spec que escribir. **CONSTRUIDA en R5-c.**
- **D-R5-17 = A** — el multiplicador se aplica sobre **`hypeDelta`**, después de
  `ComputeHypeDelta`, **no** sobre el `loopScore` crudo. Default **×1.5**, un solo loop. Corrige
  explícitamente la formulación previa ("multiplicador de LoopScore"), emitida antes de leer el
  seam: `ComputeHypeDelta` es no lineal y escalar su entrada da un efecto impredecible e
  intesteable. Constraint respetada: NO mutar `meters.SongHypeDeltaMultiplier`. **CONSTRUIDA en
  R5-c.**
- **D-R5-18 = C** — el gasto va por `StatusEffectContainer.SpendStacks`. `ConsumeOnTrigger`
  guarda `Decay == ConsumeOnTrigger`: sobre Voltage (`DecayMode.None`) sería un no-op silencioso
  — multiplicador sin gasto. `Apply(-n)` publicaría `StatusAppliedEvent` con delta negativo
  (semántica falsa en el bus). **CONSTRUIDA en R5-c.**
- **D-R5-19 = B** — el coste se paga siempre al cruzar el umbral; el factor solo si
  `hypeDelta > 0`. Preventiva: con el calculador actual los deltas negativos son inalcanzables.
  Revertir = borrar `&& hypeDelta > 0f`. **CONSTRUIDA en R5-c.**
- **Abiertas al abrir R5-d:** **D-R5-20** (convivencia disparo automático ↔ carta Action) ·
  **D-R5-21** (umbral 6 del pasivo vs ≥3 de la carta, D-R0-12) · **D-R5-22** (otras cartas que
  gasten Voltage; `SpendStacks` ya lo hace barato).
- **Fases cerradas:** R5-inv 2026-08-11 · **R5-a CERRADO 2026-08-21** (ST-R5a-1..5 + 6R PASS) ·
  **R5-b CERRADO 2026-08-21** (ST-R5b-1..6 + 7R PASS) · **R5-c CERRADO 2026-08-21**
  (ST-R5c-1..9 PASS a la primera). **R5 no cierra** — ver §3.

> **Nota de sustitución (registrada 2026-08-21, aplicada 2026-08-23 — D26).** D-R5-13/14/15
> definieron un Overload **pasivo** (automático, umbral 6, payload = multiplicador de hype). Eso
> **no** sustituye a `D-R0-5 = A` ni a `D-R5-4 = A`: se emitió sin citarlas, y el alcance original
> (carta Action + loop de bonus + solo de un loop) sigue vigente y sin construir. Lo entregado en
> R5-c es una **capa adicional** sobre el mismo recurso, no el finisher de R0. La convivencia de
> ambos disparadores se decide en R5-d (D-R5-20). Registro por evidencia de implementación, §12.

---

## 3. Batch sequence

| Batch | Mode | Scope (one line) | Phasing | Depends on |
|---|---|---|---|---|
| ~~**R0**~~ | GAME DESIGN | **CLOSED 2026-07-23.** `Design_Starter_Deck_v2` (locked): 4-musician identity map + tempo lean (v1 placeholder closed), 22-card/18-unique starter + reward slate, Levels spec closure, singalong design, §9 verifications resolved, D-R0-1..12 locked | — | — |
| **R1** ✅ **CLOSED 2026-07-23** | IMPLEMENTATION | Zig enablement: **Captivated** (amplification layer in `ApplyIncomingVibe` + `MeterTuningSO` tuning surface + SO + icon) + **Wink** card + Cantante catalog cleanup (7 legacy → `flags = None`, Wink added as sole starter entry). D-R1-1=A helper-wide amplification · D-R1-2=A MeterTuningSO home · D-R1-3 `IsBuff=false` · D-R1-4=A entries parked not deleted. ST-R1-1..6 PASS incl. demo-inertness. Authority: `SSoT_Status_Effects.md §5.8` | Interleavable | R0 |
| **R2** ✅ **CLOSED 2026-07-31** | IMPLEMENTATION | Conito enablement: profile/instruments (bass + guitars), **Finger Bass v1** + **Slap Bass v1** (`BasslineCardConfigSO` via `styleBundleCreate`), **Draw** card, Conito catalog cleanup (10/10 inert → spec), first bass-in-gig validation, **file MGP asks** (§8 #1–#3). **CLOSED 2026-07-31.** Delivered: profile (Bass backing / Guitar lead + 6-bass melodic whitelist), `InstrumentEffect_FingeredBass` + `InstrumentEffect_SlapBass`, 3 cards imported and catalog-registered, catalog cleanup. The **"file MGP asks" item is void** for #1–#3 — resolved package-side before filing (§8, boundary §8.4). Two *new* asks were filed and delivered same-cycle (boundary §8.6). | Interleavable (∥ R3) | R0 |
| **R2c** | IMPLEMENTATION | Unblocker spawned by ST-R2-1 FAIL: wire MidiGenPlay's **SOLO-1** host default progression into the gig render path as a **palette** (D-R2-6=B) + `InstrumentEffect.RandomFromList` (D-R2-7). Production runtime code; BC-gated, inert in the demo config | Interleavable | R2 |
| **R2d** | IMPLEMENTATION | Adoption of **ORDER-1** + **SLAPFIG-1** (filed and delivered 2026-07-31): guard rewrite, shared-harmony cache identity `dp:`+`bk:` (D-R2-10=A, closes the pre-existing **F-HARM-STALE-1**), harmony-source readback, Slap Bass re-authored onto `SelfPocket` (D-R2-11) | Interleavable | R2c |
| **R3** ✅ **CLOSED 2026-08-08** | IMPL / CONTENT | Zig composition cards: ascending-degree `MelodyPatternData` (verbatim `patternOverride`) + scale-phrase palette; singer verification in a 3–4-musician band (mix, channel, mute). **CLOSED 2026-08-08.** Entregado: **Rise Up** (patrón autorado de 8 compases por grado, adaptativo a raíz y a modo) y **Showtime** (ruta procedural, ST-R3-11 PASS, operativa). Entregable de banda 3–4 (mezcla/canal/mute) **CUMPLIDO**. Además en lote: **JAM-1** (continuidad de armonía compartida) y **JAM-2** (el modo viaja con la armonía), tres cartas Wormus de banco de dev (`flags=None`, D-R3C-6=A), paleta `Chord Palette - Modal` 7→5 (D-R3C-5=B). Verificaciones: ST-A1..A7 · ST-B1/B2 · ST-C1 · C5 · ST-R3-11 · C4 · ST-J1..J6, todas PASS. Excepción al freeze de baseline S5i autorizada por D-R3C-1=C / D-R3C-8=A (2º y 3er precedente). | Interleavable (∥ R2) | R0 |
| **R4** ✅ **CLOSED 2026-08-10** | IMPLEMENTATION | **CLOSED 2026-08-10.** Cuatro piezas entregadas: **Psychic Wave v2** · **C2 Spotlight/Taunt** · **Read the Room** · **Keep Cool retarget**. **ST-R4-1..10 PASS · V-R4-MODAL PASS** (salda la deuda auditiva de R3: la melodía sobre parte modal resuelve contra el modo impuesto). Ledger del lote en §2. Scope original: Finishers I: **Psychic Wave v2** (add `ApplyStatusEffect(earworm, Y≈2, AllAudienceCharacters)` — note the target branch skips `IsBlocked` members, so Indifference-blocked audience take no Earworm; full-screen mask VFX on `TutorialSpotlight.shader` base; **tutorial beat-8 + JUICE-PW regression**) + **C2 Spotlight/Taunt** (counter status + `ResolveTargetsFor` redirect hook, 1 audience turn) + **Read the Room** (`RevealPreferencesSpec` + `AudienceCharacterCanvas` surface, D-R0-1) + **Keep Cool retarget** `Self`→`Musician` (D-R0-3, **tutorial Composure-beat regression owed**) + **V5 runtime smoke** (`ApplyStatusEffect` × `AllAudienceCharacters`) | Post-S5j | S5j tag |
| **R5** 🔵 **PARCIAL** (abierto 2026-08-11; R5-a/b/c cerrados 2026-08-21) | IMPLEMENTATION | **Conito Overload** (own batch). **Entregado (R5-a/b/c):** estado contador `Voltage` (`ResourceCounter = 993`, sin decay, `MaxStacks 9`, alcance gig) · hook pasivo de generación **+1 por jugada consumida de Conito** (`GigFlowSettingsSO.GenerateVoltageOnConsumedPlay`, default ON) · `StatusEffectContainer.SpendStacks` (D-R5-18=C) · **Overload pasivo**: descarga automática en el boundary de loop, umbral 6 / coste 6, **×1.5 sobre el `hypeDelta` de ese loop** (D-R5-13/14/15/16/17). ST-R5a-* · ST-R5b-* · ST-R5c-1..9 PASS. **NO entregado, sigue siendo alcance R5:** Overload como **carta de dominio Action** (D-R0-5=A; coste 2 + Voltage ≥ 3 por D-R0-12) · API guardada de loop de bonus (`TryGrantBonusLoop` / excepción de inv 11) · **solo de un loop de Conito** (Melody, guitarra) con alcance de render (D-R5-4=A) · duck/restore de canal. **Continúa en R5-d** (ver F-R5c-4 y la nota de sustitución en §2). Abrió con la review de invariantes de sesión (R5-inv, §5 note) | Post-S5j | R2, S5j |
| **R5-d** ⚪ **PENDIENTE** | IMPLEMENTATION | Cierre del alcance R5: carta Action **Overload** + `TryGrantBonusLoop` guardada + solo de un loop por inyección de alcance de render + duck/restore. Abre con **D-R5-20** (convivencia pasivo↔carta), **D-R5-21** (umbral 6 vs ≥3) y **D-R5-22** (otros consumidores de Voltage). Los siete diffs HELD de los paquetes `PENDING_DOC_DIFFS_R5*` se aplican al cerrarlo | Post-R5-c | R5-a/b/c |
| **R6** | IMPLEMENTATION | **Double Harmony Tier A** (Harmony-role card + listening validation + dual per-track particle FX via `IMidiNoteListener`) + **`SingerVoiceDirector` one-shot API** (shared groundwork for singalong; Tier B + expression-input rider queued behind cap=2 validation) | Post-S5j | R3, S5j |
| **R7** | IMPLEMENTATION | **Track Card Levels** mechanic (state on `TrackEntry`, level-up branch in `TryAddOrReplaceTrackOnPart`, cache-invalidation duty, INSP/complexity hooks) + pilot content (Wormus Major/Minor lvl2–3). Spec: `planning/active/Design_Track_Card_Levels_v0_1.md`. May file MGP ask §8 #4 if alphabet gaps bite | Post-S5j | R0 (spec), S5j |
| **R8** | CONTENT / TEST | Rewards for all 4 (palettes via skills: jazz / Phrygian / jazz-vs-EDM drums; bossa v1 + tapping-or-degradation) + **Singalong** (on R6 one-shot API) + starter v2 registration + full-band smokes (4 musicians, full pool) + campaign doc closure | Last | R4–R7 |

Compression note: R3→R1 merge and per-musician reward distribution into R2/R3/R6 can shrink the campaign to ~7 batches at the cost of less-bounded batches. **R5 must stay solo** (invariant-touching).

---

## 4. Requirement × musician coverage map

| | Comp 1 | Comp 2 | Action | Finisher | Reward comp A | Reward comp B | Reward action (status) |
|---|---|---|---|---|---|---|---|
| **Sibi** | Wormus Major ×2 ✅ exists (+lvl 2–3, R7) | Wormus Minor ×2 ✅ exists (+lvl 2–3, R7) | Mind Tap ✅ exists; **Read the Room** (reveal) = *reward* action — R4 | Psychic Wave v2 — R4 | **Jazz Palette** (7th qualities) — R8 | **Andaluza** — Phrygian + explicit per-event qualities (V2 fallback; no enum change) — R8 | **Hive Hum** (Earworm +4, cost 1) — R8 |
| **C2** | Default Mode (4/4) ×2 ✅ exists | **Compound Cycle (6/8)** ✅ exists — promoted from pool (D-R0-2=B); Waltz Protocol → pool | Keep Cool ✅ exists, retargeted `Musician` — R4 | Spotlight/Taunt tank — R4 | **Jazz Kit** drum palette — R8 | **Neuro Kit** (d'n'b) drum palette — R8 | **Lock In** (Flow +2 Self, cost 1) — R8 |
| **Conito** | Finger Bass v1 — R2 ✅ built | Slap Bass v1 — R2 ✅ built | **Static Rush** (Draw 2 + Stress +1 Self, D-R0-4) — R2 ✅ built | Overload (Action domain, Voltage ≥ 3) — R5 | **Bossa Corda** (nylon, v1 approx) — R8 | **Tapping v1** (scale-degree arpeggio; chord-aware = ask §8 #5) — R8 | **Amp Up** (Voltage +2 Self) — R8 |
| **Zig** | **Rise Up** — ascending-degree pattern — R3 | **Showtime** — anthemic phrase palette — R3 | **Wink** (Captivated +2, cost 0) ✅ exists — R1 CLOSED | Double Harmony Tier A — R6 | **Torch Song** (ballad, slow-lean) — R8 | **Motor Mouth** (patter, fast-lean) — R8 | **Singalong** (Captivated +1 AoE, D-R0-9) — R8 |

*(All `(working)` names remain subject to the per-batch naming pass.)*

---

## 5. Verdict table (feasibility, 2026-07-23 session)

Effort: **A** = authoring/content only · **B** = authoring + bounded ALWTTT code · **C** = own runtime batch · **MGP** = MidiGenPlay feature (out of scope here; ask).

| Item | Verdict | Scope | Effort |
|---|---|---|---|
| Sibi Wormus Major / Minor | Already shipped (v1.1, ×2 each, major/minor `progressionPalette`) | — | 0 |
| Sibi reveal action | Taste data exists (`TastePreferences`, 4 axes: Tempo / density / TS / Tonality). Needs new `CardEffectSpec` (`RevealPreferencesSpec`) via `SSoT_Card_Authoring_Contracts §9` four-layer rule + `AudienceCharacterCanvas` TMP surface. **Name collision:** existing Mind Tap = `ModifyVibe(+5)` + `Earworm(+2)` | ALWTTT | B |
| Sibi Psychic Wave v2 | Psychic Waves exists (cost 3, `ModifyVibe +5, AllAudienceCharacters`). Add `ApplyStatusEffectSpec(earworm, Y, AllAudienceCharacters)` — target verified in spec + `CardBase`. VFX: full-screen circular mask + color inversion; `TutorialSpotlight.shader` reusable base. **Regression duty:** tutorial beat 8 + JUICE-PW presentation (per-target `AudienceVibeImpactEvent`, `CardVibeImpact` sting) | ALWTTT | A payload + B VFX |
| C2 4/4 | Default Mode exists; optional `DrumPatternPaletteSO` upgrade (palette runtime wired 2026-06-04) | ALWTTT | 0–A |
| C2 6/8 | **Resolved D-R0-2=B:** Compound Cycle promoted **to starter** (flags-only change; card already authored, `MeterEffect(6/8)`, gen 3), Waltz Protocol demoted to reward pool. Blind-listener contrast for C2's starter pair improves (simple duple vs compound triple). No tutorial reference to either card | ALWTTT | 0 (flags) |
| C2 targetable Composure | Keep Cool exists (`Self`). `targetType = Musician` variant is pure authoring (verified) | ALWTTT | A |
| C2 Spotlight/Taunt finisher | New status + redirect hook in `AudienceCharacterBase.ResolveTargetsFor` (`Musician`/`RandomMusician` → C2, 1 audience turn). No CSO redirect primitive → append (enum append-only) or bespoke key + runtime check. VFX = animation trigger | ALWTTT | B/C |
| Conito Finger Bass | Bassline role + `BasslineCardConfigSO` exist (figures: Block, arpeggio pulse, Offbeat stabs, PerBeat). **1st–5th–8ve–3rd walk NOT implemented** — recorded package-side candidate (seeded-variation batch, CA roadmap) → ask §8 #1 | ALWTTT v1 / MGP walk | A + ask |
| Conito Slap Bass | Timbre = patch (Slap Bass 1/2 in soundfont). Octave/pulse ≈ Offbeat/PerBeat. **Rhythm-track following (pocket) unsupported** (bass renders shared progression, single pass, no cross-track read; `patternOverride` on bass = warn+ignore D-DBG4=A) → ask §8 #2 | ALWTTT v1 / MGP pocket | A + ask |
| Conito Draw X | `DrawCardsSpec` exists (Warm Up). Identity overlap with the generic → refine in R0 (draw + rider) | ALWTTT | A |
| Conito Overload finisher | Counter status (Additive, no decay — supported) + threshold hook (new) + bonus loop (`_loopsRemainingForPart++` trivial per the dev infinite-loop precedent, but needs a guarded API: final-loop lock inv 11, per-loop-insp exclusion — D-CSV-24 economy-neutral pattern as precedent for the solo track, F-3 draws, ECON-1 Seam-C refill on the extra loop, TLM-1 loop counts) + solo track (Conito Melody, guitars via `profile.leadInstruments`) + duck/restore (`SetChannelVolume`; Highlight×mute-family risk) + **one-loop-scoped revert (new pattern** — mutations are persistent in part cache today) | ALWTTT | **C** |
| Zig ascending-note comp | `MelodyCardConfigSO.patternOverride` plays `MelodyPatternData` verbatim; **patterns are degree-based** (ScaleDegree + octave offset, pitch resolved vs Part tonality/root — verified) → key/mode-adaptive by construction. Sung: Pink Trombone glide (`pitchLeadSeconds`/`leadFullInterval`) renders the sweep. Verify pattern-Measures vs part length at authoring | ALWTTT | A |
| Zig scale-phrase comp | `PhrasePaletteSO` + existing archetypes (EvenFlow / BurstThenHold / SustainLeadIn) + `MelodicStyleSO` | ALWTTT | A |
| Zig Wink (Captivated) | Designed (`Design_Audience_Status_v1 §4`, `DamageTakenUpMultiplier`, ×(1+0.25N)). `ApplyIncomingVibe` already canonical → only the amplification layer + SO + icon + card remain | ALWTTT | B (small) |
| Zig Double Harmony | **Tier A:** Harmony role exists end-to-end package-side (`HarmonyTrackComposerFactory`, `NearestDifferentChordTone`, two-pass orchestration reading Melody guide notes D-MEL4.4; readback does not report Harmony ID-2=A → **listening validation owed at batch open**). **Tier B:** second sung voice = slot-2 intended consumer; needs Director role-filter extension (Melody/Lead only today) + cap=2 validation (deferred to Dev Mode) + 2-voice DSP budget | ALWTTT | A/B (Tier A) · B/C (Tier B) |
| Track Card Levels | New mechanic; spec note `Design_Track_Card_Levels_v0_1.md`. Alphabet verified rich enough for the lvl3 exemplar minus slash chords (§9 V1) | ALWTTT | C (R7) |
| Fill Window (C2) | Registered idea, post-campaign — `planning/Design_Fill_Window_v0_1.md` | cross-cutting | C+ |
| Singer expression input | Registered idea, post-campaign (candidate Tier-B rider) — `planning/Design_Singer_Expression_Input_v0_1.md` | ALWTTT | B |
| Singalong (Zig reward action) | Mini scripted event: short authored phrase → `SingerVoiceDirector` **one-shot API** (new consumer-side entry; today the Director arms only from `LoopPlaybackStarting`) → crowd response as GM Choir-Aahs echo + crowd SFX (cheap path; avoids voice-2 budget and the open singer mixer-bypass follow-up). Pre-song PlayerTurn window. Gameplay effect + carried status TBD in R0 | ALWTTT | B/C (R8, on R6 API) |

---

## 6. Reward slate — collisions and gaps (input to R0)

- **Sibi:** jazz chord palette · Phrygian/Phrygian-dominant/flamenco palette (Phrygian confirmed in the tonality enum via Wormus Minor; **Phrygian dominant unverified** → V2; fallback = authored Andalusian-cadence progressions by degree/quality) · Earworm action. Existing in pool: **Vamp** (+INS) — R0 decides whether it counts toward the slate.
- **C2:** proposal resolving the 3/4–5/4 collision (3/4 = starter Waltz Protocol; 5/4 = reward Pentameter already): the two path slots = **jazz vs EDM (d'n'b) `DrumPatternPaletteSO` cards** (skill `rhythm-pattern-generator` covers both genres); Pentameter retained as an existing extra. Existing in pool: **Compound Cycle, Pentameter, In the Pocket**.
- **Conito:** bossa backing on nylon guitar (v1 approx: arpeggio/offbeat figure + nylon patch + suitable palette; true bossa split = ask §8 #3) · tapping melody arpeggiating the *current* chord — **chord-aware resolution does not exist** (degree patterns resolve vs tonality/root, not vs the progression event; §9 V3) → v1 = scale-degree arpeggio figures that fit the palette's progressions, or ask §8 #5 · Overload-synergy action.
- **Zig:** **two composition-reward directions UNDEFINED** (gap — R0 must propose) · Singalong as the reward-action slot (must carry a status per the requirement — candidate: Earworm-to-all or Captivated; R0 decides).
- **Cross-cutting:** +INSP-per-level (Levels mechanic) overlaps the existing +INSP lever (Vamp / In the Pocket, `AddInspirationPerLoopSpec`). R0 resolves coexist / replace / reserve.

---

## 7. New mechanics and registered ideas

> **Design backlog — R3 (2026-08-08). Ideas registradas, NINGUNA comprometida.**
> *(Material de planificación. No es autoridad y no dirige implementación.)*
>
> 1. **Carta "transport"** (D-R3C-2, variante B) — una carta cuyo propósito explícito sea
>    mover la canción entera a otra tonalidad transportando la armonía existente en vez de
>    reemplazarla. Arrastrada desde el handoff §6 D7. **Idea, no comprometida.**
> 2. **Familia de cartas articulation-only.** Diez figuras de `ChordExpressionType` son
>    autorables hoy con cero código, y JAM-2 hace el arquetipo seguro también en contextos
>    modales ⇒ diez cartas de estilo posibles sin trabajo de runtime. Contrato de autoría y
>    trampa de nombres `BassUpperSplit` vs `Bossa`: `SSoT_Card_Authoring_Contracts.md` §5.18.

- **Track Card Levels** — in campaign (R7). Spec + expressibility analysis: `planning/active/Design_Track_Card_Levels_v0_1.md`. Solves the dead-composition-card problem (re-playing an already-rendered card levels the track instead of doing nothing meaningful).
- **Fill Window** — registered, **not scheduled**: `planning/Design_Fill_Window_v0_1.md`. End-of-loop timed window for fill cards; conflicts with the "mutations never touch the playing loop" invariant → overlay-vs-next-loop analysis in the note. Candidate C2 "path" post-campaign; the windowed-timing primitive is reusable.
- **Singer Expression Input** — registered, **not scheduled**: `planning/Design_Singer_Expression_Input_v0_1.md`. Player input drives live voice levers; the SSoT's "concrete consumer" condition is met by design here. Natural rider of Double Harmony Tier B.

---

## 8. MidiGenPlay asks

> **Estado post-R3 (2026-08-08).** Task A **hecha**, Wormus Modal **hecha**. Cola restante:
> **LOG-1 → tag snapshot → R4+**. Con `snapshot-01` cortado (2026-08-08), R4+ queda
> **DESBLOQUEADO** bajo D-SEQ-3=A.
>
> **Decisiones y verificaciones que R3 deja debidas:**
> - **D-R4-1 — ¿el público juzga la tonalidad autorada o la que suena?** `LoopFeedbackContext`
>   se construye desde el modelo de UI, así que bajo armonía modal la audiencia evalúa Ionian.
>   Es una pregunta de **diseño**, no un defecto. **Estado tras R4 (2026-08-10):** dirección
>   decidida (RECONOCER, NO CONSUMIR — el público debe juzgar lo que suena); cableado
>   **prohibido**; lote propio, antes de R8 (R8 autora contenido que asume la semántica).
> - ~~**Verificación diferida:** comprobación auditiva de melodía sobre parte modal.~~
>   **CERRADA en R4 (V-R4-MODAL PASS, 2026-08-10):** la melodía sobre parte modal resuelve
>   contra el modo impuesto. La deuda auditiva de R3 queda saldada.
>
> **Criterio de aceptación de C4 — reformulado.** La variación se mide **entre renders**, no
> entre loops: un loop repetido replaya bytes cacheados por construcción, así que exigir
> variación loop-a-loop mediría la caché, no el composer.
>
> - **MGP-MEL-1** (enviado 2026-08-05) — pipeline de melodía, 8 puntos. P1 selección de altura
>   estancada (era bloqueante de Showtime), P2 campos serializados inertes, P3 observabilidad
>   del leading efectivo, P4 progresiones modales vs tonalidad de la parte, P5 viabilidad de
>   "Rise Up adaptativa", P6 refinamiento de la superficie de autoría, P7 propiedad de la
>   progresión al añadir pistas a un jam en marcha, P8 `totalSlotsInPhrase` inconsistente.
>   Registro de frontera: `SSoT_ALWTTT_MidiGenPlay_Boundary.md` §8.9.
> - **O4 / evidencia debida a MidiGenPlay:** `MGP_Evidence_Bundle_from_ALWTTT_R3_2026-08-08.md`
>   (cuatro asks package-side).

Filing rule: asks are filed **with acceptance criteria** at the batch that owns the demand (R2 for #1–#3, R7/R8 for #4–#5), never as intentions. They join the existing pending item `MGP-ALWTTT-ARTIC-1` (DF-ARTIC) in the cross-boundary queue. None are redesigned here (boundary rule).

1. ~~Bass chord-tone walk figures~~ — **RESOLVED package-side 2026-07-28 before filing.**
   Delivered as `arpeggioToneMode`. Adopted R2 (D-R2-4=A). Boundary §8.4.
2. ~~Bass pocket-coupling~~ — **RESOLVED package-side 2026-07-28 before filing.** Delivered as
   `pocketMode = SlapPocket` (+ POCKET-2). Superseded for ALWTTT by `SelfPocket` (§8.6);
   `SlapPocket` retains a dormant consumer-side cache duty (§8.4).
3. ~~Bossa bass/upper split~~ — **RESOLVED package-side 2026-07-28 before filing.** Delivered as
   `ChordExpressionType.BassUpperSplit` (**not** the member named `Bossa` — naming trap, §8.4).
   Consumer is R8 *Bossa Corda*.
4. **Conditional — bass-side `degreeAccidental` consumption:** narrowed at R0. V1-residual passed structurally and V2's failure is absorbed by an ALWTTT-side fallback (authored Andalusian progressions over Phrygian), so neither triggers an ask. V4 showed the **backing** composer already honors accidentals on both render paths while the **bass** ignores them — the only remaining demand is bass-side parity, and it is triggered only if level or reward content ever needs chromatic roots in a band containing Conito. Level content is diatonic-root by decision, so this stays conditional. *(R7)*
5. **Conditional — chord-aware melody resolution** (pattern degrees resolved against the sounding chord, or an arpeggio melody strategy) for the tapping reward. *(R8)*

**Filed and delivered 2026-07-31 (boundary §8.6), both adopted:**
- `MGP-ALWTTT-BASS-ORDER-1` — shared harmony independent of track order; five-level precedence;
  new `sharedProgressionSource` readback. Closed **F-BASS-ORDER-1**.
- `MGP-ALWTTT-BASS-SLAPFIG-1` — `PocketCouplingMode.SelfPocket`, autonomous slap/pop, no
  cross-track read.
Also previously delivered unrequested and adopted: `MGP-ALWTTT-BASS-SOLO-1` (§8.5).

Items 4–5 (conditional chord alphabet / chromatic degrees; chord-aware melody resolution) remain
**open and unfiled**, owned by R7 / R8. Filing rule unchanged.

**Registered, not filed:** an `Auto` pocket mode (SelfPocket without drums, coupling to Rhythm
when present). Deliberately not requested — conditional cross-track coupling re-arms the §8.4
cache duty, so it costs a consumer-side batch, and a mid-song figure change may read as a bug
rather than adaptation. Revisit alongside SLAPFIG-2 when real drum content exists.

---

## 9. Verifications — RESOLVED at R0 (2026-07-23)

Method + code citations: `Design_Starter_Deck_v2.md` §7.

- **V1 — chord quality alphabet: RESOLVED.** Full alphabet parses; per-event quality reaches voicing at both backing render sites and on the melody chord-tone path. Slash inversions remain inexpressible (voicer owns inversions). Residual reduced to an interval-table audit + audible spot-check → **R7 pilot smoke**.
- **V2 — Phrygian dominant: FAIL, fallback locked.** The `Tonality` enum is the seven diatonic modes only. The flamenco sound is authored instead as explicit degree+quality events over `Tonality.Phrygian` (the Andaluza reward card). **No MGP ask triggered.**
- **V3 — melody patterns: RESOLVED.** Degree-based (tonality-adaptive) confirmed; the authored loop **tiles by raw beats to the part length**, truncating the final partial repeat, warning on meter mismatch (D-MEL5.1=A). Authoring guidance: author the pattern in the card's expected meter. Chord-aware resolution still absent → tapping reward gates on ask #5 or degrades.
- **V4 — `degreeAccidental`: recorded gap CORRECTED.** Backing honors it on both paths (marker-parity tested); bass ignores it. Constraint (diatonic-root level content) stands, but for band-composition reasons. Ask #4 narrowed accordingly. **Cross-boundary doc note owed** — filed 2026-07-31; text preserved in `CSV_Composition_Validation_Sub_Roadmap.md` §5 (the R0 diff file is retired).
- **V5 — `ApplyStatusEffectSpec` + `AllAudienceCharacters`: verified and CLOSED (runtime smoke run at R4, 2026-08-10).** Target resolution is **per spec** — each spec calls `DetermineTargets` and resolves its own list (equivalent for `All*` targets, **not** for `Random*`). The `AllAudienceCharacters` branch is live for Psychic Wave v2's `ModifyVibe` + `ApplyStatusEffect` and **excludes `IsBlocked` members**. *(Corrected at R4 / F-R4-1: this record previously claimed "one shared target list serves all specs on a card", which the code does not do. V5's result stands; only its description changed. Authority: `SSoT_Card_System.md` §8.2.)*

## 10. Open items at R0 — RESOLVED

All items are closed by **D-R0-1..12** (§2, R0 ledger). Residual work is owned per batch and listed in `Design_Starter_Deck_v2.md` §8 (interval-table audit → R7 · ~~V5 smoke → R4~~ **cerrado R4 2026-08-10** · dual-melody mix validation → R3/R8 · ~~Keep Cool tutorial regression → R4~~ **cerrado R4, ST-R4-9 PASS** · draw/hand economy retune for 22 cards × 4 musicians → R8 · action:composition ratio observation → R8 · naming passes → per batch · finisher cost tuning → R8).

---

## 11. R1 rehydration prompt — superseded (record)

R0 closed with an R1 rehydration prompt issued at session close (2026-07-23); this section was to carry it
verbatim (R0 doc package, P2.9). That prompt was consumed: **R1 closed 2026-07-23** and **R2 + R2c + R2d
closed 2026-07-31** (§3), and its verbatim text was not preserved in the PK by the time the R0 package was
applied (2026-07-31, DOC-APPLY-1). It is deliberately not reconstructed (no-invention rule). The next prompt
owed by this section is the **R3** prompt, to be issued at R3 open.

---

## 12. Update rule

Update this document at every campaign batch open/close (status column, decisions promoted from `D-RX-*` ledgers, asks filed, verdicts corrected by implementation evidence). When R8 closes: this doc's batch table becomes historical record; `Design_Starter_Deck_v2` authored assets become runtime-authoritative (same lifecycle as v1); update `Roadmap_ALWTTT.md`, `CURRENT_STATE.md`, `changelog-ssot.md`, and retire the campaign from "Next active".

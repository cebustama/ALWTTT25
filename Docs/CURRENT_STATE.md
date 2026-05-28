# Addendum para CURRENT_STATE.md §1

**Acción:** insertar este bloque al TOPE de §1 ("What just changed"), inmediatamente arriba del bloque más reciente (B3-content-cards 2026-05-22).

---

### Phase B B3 follow-on — ALWTTT-MOD-DIR-2 + ALWTTT-MOD-DIR-3 — complete (2026-05-22)

Audible-polish follow-on to B3-content-cards' Key Lift card. Two cross-project batches landing together on the ALWTTT side, accompanied by three companion closures on the MidiGenPlay side (MGP-ALWTTT-MOD-DIR-1, 1.1, 1.2). Together they ship a directional intent for `ModulationEffect`: cards can now author Up/Down direction for the first chord of the post-modulation render, and the audible result matches the authored intent.

**ALWTTT-MOD-DIR-2** — adopted the MidiGenPlay directional surface (six decisions resolved: D-A1=A reuse package `MidiGenPlay.Composition.ModulationOctaveHint` enum directly; D-A2=A capture previous root locally in `ApplyEffectToModel` before mutation; D-A3=B stage on `PartEntry`, write+clear at `SongConfigBuilder`; D-A4=no `CompositionCardClassifier` change; D-A5=A expose `octaveHint` on SO inspector with default `Auto`; D-A6=B append glyph suffix in `GetLabel()` for non-`Auto` direction). `ModulationEffect.cs` gained `octaveHint : ModulationOctaveHint` field; `SongCompositionUI.PartEntry` gained two `[NonSerialized]` staging fields; `SongConfigBuilder.Build()` writes-and-clears the staged transients onto `PartConfig` at the sole handoff site. `ModulationEffect_KeyLift_Degree5.asset` re-authored with `octaveHint = Up`.

**ALWTTT-MOD-DIR-3** — fixed a cache-layering bug discovered during SM-DIR-5 verification. `MidiMusicManager._partBundleCache` keys on `partMeterHash` + per-track input hashes, which does not include the `[NonSerialized]` `PartConfig` transients. Same-root modulations (degree=Tonic + non-Auto hint) hit the cache and replayed pre-modulation bytes verbatim, silently dropping the directional intent. Fix: `RenderSinglePart` forces `cacheEnabled = false` when either transient is non-default. Bypass is one-shot — composer consumes and clears the transients in the same call, so a subsequent Auto render caches and replays normally. New `[Mod-DIR/CacheBypass]` log (gated on `logDebug`) provides production observability for future cache investigations.

**Smoke tests.** SM-DIR-1 (strict Up Dominant) PASS. SM-DIR-2 (strict Down Dominant) PASS. SM-DIR-3 (Auto regression, bit-identical baseline) PASS. SM-DIR-4 (chained Up modulations) PASS. SM-DIR-5 (Tonic + Up register bump) PASS after the cache-bypass fix. SM-DIR-6 (transients clear after consumption) PASS. SM-DIR-7 (range-clamp fallback) deferred — requires narrow-range debug instrument tooling not yet present.

**Demo impact.** Key Lift now audibly lifts. Pre-batch the card shifted pitch class (C → G) but the voice leader picked minimum-distance octave, so "Up a fifth" could land G3 below the previous C5 tonic — directionally ambiguous. Post-batch the first chord of the modulated render lands strictly above the previous tonic, matching the card's authored name and player intuition. No deck content changes beyond the asset re-author.

**Files modified.** `ModulationEffect.cs`, `SongCompositionUI.cs`, `SongConfigBuilder.cs`, `MidiMusicManager.cs`, `ModulationEffect_KeyLift_Degree5.asset`, `integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md`, `changelog-ssot.md`. No SSoT contract change ALWTTT-side. No `CURRENT_STATE.md` §3 rotation (B3-slate remains next-active conceptually but is superseded by the new S1-S8 sequencing produced in the 2026-05-23 planning reframe).

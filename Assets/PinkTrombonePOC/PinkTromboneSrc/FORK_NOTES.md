# Pink Trombone source fork — POC-local (D-POC-8 option A)

Forked from `github.com/lostmsu/pink-trombone-mod`, tag **v0.1.0** (the NuGet build),
on 2026-07-18. Upstream is MIT-licensed (Neil Thapen; C# port Victor "lostmsu" Nova;
filter code Christian d'Heureuse) — see `LICENSE.txt` alongside this file, which must
travel with the source.

**Scope guardrail:** this fork lives inside the `MidiGenPlay.PinkTrombonePOC` asmdef
ONLY. It must never enter `MidiGenPlay.Runtime`. The package emits MIDI; it does not
own audio synthesis.

## Changes vs upstream (all marked `// POC-FORK(n)` in source)

| # | File | Change | Why |
|---|------|--------|-----|
| 1 | `Glottis.cs`, `PinkTrombone.cs` | Split the single `loudness` field into internal `waveformLoudness` (tenseness^0.25, recomputed per glottal cycle — stock timbre path unchanged) and `userLoudness` (public `Loudness`, default 1). Waveform now multiplies both. | Upstream `SetupWaveform` clobbered the public `Loudness` every glottal period (~440×/s at A4), making it write-only. MIDI velocity needs a real, timbre-preserving amplitude control. |
| 2 | `TractShaper.cs` | `TongueIndex`/`TongueDiameter` setters raise a dirty flag; `AdjustTractShape` refills `targetDiameter[]` (targets only) from `GetRestDiameter` when dirty. | Upstream computed `targetDiameter[]` only in the constructor, freezing the vowel at the defaults forever. Deliberate deviation from the rehydration prompt's "call `ShapeMainTract()`" suggestion: `ShapeMainTract` also snaps the *live* `tract.diameter[]`, which would kill the articulatory glide; refilling targets only lets the existing `MoveTowards` (speed 15) slide the vowel. |
| 3 | `PinkTrombone.cs` | Exposed `IsTouched` and `AlwaysVoice` (already on `Glottis`) on the facade. | Drives the model's internal intensity envelope (+0.13/block on, −0.05/block off): note-on/off articulation inside the model, breath persisting through rests. |
| 4 | `PinkTrombone.cs` | Deleted the per-block `Debug.WriteLine` in `SynthesizeBlock`; dropped `using System.Diagnostics`. | Unity editor defines `DEBUG`, so a source import would log ~90×/s. |
| 5 | `RandomSource.cs` (new), `Glottis.cs`, `Noise.cs`, `Tract.cs`, `PinkTrombone.cs` | Replaced `Troschuetz.Random.IGenerator` with a vendored `Vocal.IRandomSource` + `StandardRandomSource` (System.Random-backed). Removed three `using Troschuetz.Random;` lines. | The dependency surface was two members (`Seed`, `NextDouble()`). Troschuetz.Random existed in this project only as a transitive dependency of the PinkTrombone NuGet package; removing that package (mandatory, to avoid a duplicate `Vocal.PinkThrombone` type) removed it too, and it does not surface as a standalone package in NuGetForUnity's search. Vendoring removes the external dependency entirely. |
| 6 | `Glottis.cs`, `PinkTrombone.cs` | Added `PitchJitterGain` (0..1, default **1**) gating the two always-on simplex F0 jitter terms in `CalculateVibrato` (`0.02·S(4.07t) + 0.04·S(2.15t)`). The gain multiplies **each term separately**, not a grouped sum, so gain = 1 preserves upstream's float evaluation order (verified bit-identical). | These two terms sat outside BOTH the `AutoWobble` gate and `VibratoAmount` — an always-on, unreachable F0 wander at 2–4 Hz, measured at **34.7 cents SD / 150 cents peak-to-peak** on an 8 s sustain (48 kHz, seed 12345, vibrato off). This was the POC's residual "out of tune" instability; four sessions of control-layer work could not touch it. At gain = 0 the same sustain measures **0.01 cents SD**. |
| 7 | `Glottis.cs`, `PinkTrombone.cs` | Added `TensenessJitterGain` (0..1, default **1**) gating the always-on simplex tenseness drift in `CalculateNewTenseness` (`0.1·S(0.46t) + 0.05·S(0.36t)`), same per-term multiplication for float parity. | The drift spans ±~0.11 — more than half the settled 0.40–0.60 tenseness window — continuously wandering Rd (glottal waveform shape), spectral tilt, and `waveformLoudness` (tenseness^0.25) with no way to stop it. Secondary contributor to perceived pitch/timbre vagueness. |

Not copied: `InternalsVisibleTo.cs` (test-assembly plumbing), `PinkTrombone.csproj`.
All other files (`Tract.cs`, `Noise.cs`, `NoiseGenerator.cs`, `MathX.cs`, `Arg.cs`,
`Transient.cs`, `TurbulencePoint.cs`) are verbatim upstream.

## Verification performed at fork time (console harness, mono, 48 kHz)

- Compiles standalone with **no external references at all**; all four repairs re-verified
  after the dependency was vendored (identical peaks, tongue delta and articulation).
- **A/B vs untouched upstream at defaults (`Loudness = 1`): bit-identical over 96 000
  samples (max delta 0).** Stock timbre path provably unchanged.
- `Loudness = 0.25` → tail peak ratio 0.252 (linear, working).
- Tongue change after synthesis start → tail waveform diverges strongly (un-frozen).
- `AlwaysVoice = false`, `IsTouched = false` → output decays to silence; `IsTouched =
  true` → full voice returns. Internal articulation working.

## Verification of POC-FORK(6/7) (dotnet 8, 48 kHz, seed 12345)

- **Bit-identical at defaults**: 96 000 samples, patched vs pre-patch source, max delta 0.
- **Bit-identical at gain = 1** in the sustain config (vibrato 0, wobble off, 220 Hz):
  384 000 samples, max delta 0. The parity guarantee from fork time still holds.
- F0 wander on an 8 s sustain (autocorrelation, 100 ms windows, first 1.5 s skipped):
  gain 1 → 34.7 cents SD / 150.7 cents p2p; gain 0.15 → 5.2 / 22.8; gain 0 → 0.01 / 0.1.
- Setters use `Check01` and **throw** outside 0..1, like the other 0..1 properties —
  callers clamp before assignment.

## Dependencies
**None from NuGet.** All three original packages (`PinkTrombone`, `Troschuetz.Random`,
`System.Memory`) can be uninstalled. `Span<T>` comes from the .NET Standard 2.1 profile
(Api Compatibility Level must stay at **.NET Standard 2.1**); randomness is vendored in
`RandomSource.cs` (POC-FORK 5). If `Span`/`ReadOnlySpan` errors ever appear, that means
the compatibility level was lowered — fix the level rather than reinstalling `System.Memory`.

Callers construct the voice as:
`new PinkThrombone(AudioSettings.outputSampleRate, new StandardRandomSource())`
(was `new StandardGenerator()`).

## Caveat on the noise stream
The bit-identical A/B above was run against a `System.Random`-backed stub of
Troschuetz's `StandardGenerator` (which itself wraps `System.Random`). The vendored
`StandardRandomSource` is equivalent in kind, but the exact aspiration/simplex noise
sample stream may differ from the real Troschuetz DLL. This affects the realization of
the noise, not the model's behavior or timbre.

## Thread-safety note
`tongueDirty` is written on the main thread and consumed on the audio thread; the
benign race delays a recompute by at most one block. POC-tier acceptable.
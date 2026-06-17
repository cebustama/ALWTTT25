using System;
using System.Collections.Generic;

using MidiGenPlay.Composition;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Resolves a palette INTENT ("a 6/8 palette", "something funky") to one
    /// concrete project palette, deterministically (CE-L1, D-CE-L1.2).
    ///
    /// This is the asset-intent boundary the roadmap mandates: the LLM never
    /// emits a palette asset name; it emits a meter + keywords, and this
    /// resolver picks among real palettes. It composes over the CE-F1 selector
    /// rather than extending it:
    /// <list type="number">
    ///   <item><description>keyword pre-filter (ALL keywords must match, case-insensitive
    ///   substring of DisplayName + Notes). Empty result is a HARD failure that
    ///   lists the available palettes — no silent fallback (SSoT §3.3).</description></item>
    ///   <item><description>survivors sorted by <see cref="PaletteDescriptor.Id"/> (ordinal)
    ///   so project-scan order can never change the pick.</description></item>
    ///   <item><description>with a desired TS: each palette becomes one
    ///   <see cref="Candidate{T}"/> (weight 1) represented by its BEST entry —
    ///   an exact-TS entry if one exists (so Tier A triggers), else the entry
    ///   with the highest <see cref="PaletteSelector.ComputeTsHeuristicMultiplier"/>.
    ///   The pick is <see cref="PaletteSelector.Pick{T}"/> with preferExactTs=true —
    ///   the same Tier A/B/C policy, one rng.NextDouble per pick.</description></item>
    ///   <item><description>without a desired TS: a single seeded uniform draw among
    ///   survivors (also exactly one rng.NextDouble).</description></item>
    /// </list>
    /// Determinism: same seed + same descriptor set (any order) + same intent
    /// ⇒ same pick.
    /// </summary>
    public static class CardPaletteIntentResolver
    {
        /// <summary>Immutable resolution outcome.</summary>
        public readonly struct Result
        {
            public readonly bool Success;

            /// <summary>Id of the chosen palette (asset path), valid when Success.</summary>
            public readonly string ChosenId;

            /// <summary>Display name of the chosen palette, valid when Success.</summary>
            public readonly string ChosenDisplayName;

            /// <summary>Human-readable warnings/info for the editor warning panel.</summary>
            public readonly IReadOnlyList<string> Warnings;

            public Result(bool success, string chosenId, string chosenDisplayName, IReadOnlyList<string> warnings)
            {
                Success = success;
                ChosenId = chosenId;
                ChosenDisplayName = chosenDisplayName;
                Warnings = warnings ?? Array.Empty<string>();
            }
        }

        /// <summary>
        /// Resolve an intent against the given palettes.
        /// </summary>
        /// <param name="palettes">Candidate palettes (any order; resolver sorts internally).</param>
        /// <param name="desiredTs">Meter preference, or null for "any".</param>
        /// <param name="keywords">Keywords that must ALL match DisplayName+Notes; null/empty = no filter.</param>
        /// <param name="minHarmonicSubdivisions">CE-F1 Tier-B knob (MidiGenPlayConfig.minHarmonicSubdivisions; 4 when unknown).</param>
        /// <param name="rng">Seeded RNG. Exactly one NextDouble is consumed on the success path.</param>
        public static Result Resolve(
            IReadOnlyList<PaletteDescriptor> palettes,
            TimeSignature? desiredTs,
            IReadOnlyList<string> keywords,
            int minHarmonicSubdivisions,
            System.Random rng)
        {
            rng ??= new System.Random();
            var warnings = new List<string>();

            // ---- sanitize ----
            var usable = new List<PaletteDescriptor>();
            if (palettes != null)
            {
                foreach (var p in palettes)
                {
                    if (p == null) continue;
                    if (p.Entries == null || p.Entries.Count == 0)
                    {
                        warnings.Add($"Palette '{Name(p)}' skipped: it has no entries.");
                        continue;
                    }
                    usable.Add(p);
                }
            }

            if (usable.Count == 0)
                return Fail(warnings, "No usable palettes available for this role. Author a palette first.");

            // ---- keyword pre-filter (ALL must match) ----
            var filtered = usable;
            if (keywords != null && keywords.Count > 0)
            {
                filtered = new List<PaletteDescriptor>();
                foreach (var p in usable)
                {
                    string haystack = (p.DisplayName ?? "") + "\n" + (p.Notes ?? "");
                    bool all = true;
                    foreach (var kw in keywords)
                    {
                        if (string.IsNullOrWhiteSpace(kw)) continue;
                        if (haystack.IndexOf(kw.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            all = false;
                            break;
                        }
                    }
                    if (all) filtered.Add(p);
                }

                if (filtered.Count == 0)
                {
                    return Fail(warnings,
                        $"No palette matches keywords [{string.Join(", ", keywords)}]. " +
                        $"Available: {ListNames(usable)}. " +
                        "Adjust the brief or author/rename a palette — intent is never silently dropped.");
                }
            }

            // ---- deterministic order: scan order must not matter ----
            filtered.Sort((a, b) => string.CompareOrdinal(a.Id ?? "", b.Id ?? ""));

            // ---- no meter preference: single seeded uniform draw ----
            if (!desiredTs.HasValue)
            {
                int idx = (int)(rng.NextDouble() * filtered.Count);
                if (idx >= filtered.Count) idx = filtered.Count - 1; // NextDouble() < 1.0, defensive anyway
                var pick = filtered[idx];
                return new Result(true, pick.Id, Name(pick), warnings);
            }

            // ---- meter preference: palette-level Tier A/B/C via PaletteSelector ----
            var ts = desiredTs.Value;
            bool anyExact = false;

            var candidates = new List<Candidate<PaletteDescriptor>>(filtered.Count);
            foreach (var p in filtered)
            {
                var rep = RepresentativeFeatures(p, ts, minHarmonicSubdivisions, out bool exact);
                anyExact |= exact;
                candidates.Add(new Candidate<PaletteDescriptor>(p, 1f, rep));
            }

            if (!anyExact)
            {
                warnings.Add(
                    $"No palette has an exact {ts} entry; picked the nearest by the " +
                    "CE-F1 meter heuristic (bar length / beat unit / parity / density).");
            }

            var chosen = PaletteSelector.Pick(
                candidates, ts,
                preferExactTs: true,
                minHarmonicSubdivisions: minHarmonicSubdivisions,
                rng: rng,
                verbose: false,
                label: "CARD_PALETTE_INTENT");

            if (chosen == null)
                return Fail(warnings, "Palette selection returned nothing (unexpected: candidates were non-empty).");

            return new Result(true, chosen.Id, Name(chosen), warnings);
        }

        /// <summary>
        /// The palette-level feature summary handed to <see cref="PaletteSelector.Pick{T}"/>:
        /// the palette is represented by its best entry for the desired meter — an
        /// exact-TS entry when one exists (Tier A then applies to the palette), else
        /// the entry with the highest Tier-B multiplier. Features are computed with
        /// the same CE-F1 helpers the typed finders use
        /// (<see cref="PaletteSelector.StartsPerBar"/> for chords,
        /// <see cref="PaletteSelector.DrumStartsPerBar"/> +
        /// <see cref="PaletteSelector.DefaultGroupingCount"/> for drums).
        /// </summary>
        internal static TsFeatures RepresentativeFeatures(
            PaletteDescriptor p,
            TimeSignature desiredTs,
            int minHarmonicSubdivisions,
            out bool hasExact)
        {
            hasExact = false;
            int groupCount = PaletteSelector.DefaultGroupingCount(desiredTs);

            TsFeatures best = default;
            float bestScore = float.NegativeInfinity;
            bool any = false;

            foreach (var e in p.Entries)
            {
                var f = FeaturesFor(e, p.IsDrumDomain, groupCount);

                if (f.TimeSignature == desiredTs)
                {
                    // First exact entry wins outright (entry order, stable).
                    hasExact = true;
                    return f;
                }

                float score = PaletteSelector.ComputeTsHeuristicMultiplier(f, desiredTs, minHarmonicSubdivisions);
                if (!any || score > bestScore)
                {
                    any = true;
                    bestScore = score;
                    best = f;
                }
            }

            return best;
        }

        internal static TsFeatures FeaturesFor(PaletteEntryDescriptor e, bool isDrum, int groupCountForDesiredTs)
        {
            float startsPerBar = isDrum
                ? PaletteSelector.DrumStartsPerBar(e.StructuralOnsets, e.Measures, groupCountForDesiredTs)
                : PaletteSelector.StartsPerBar(e.StructuralOnsets, e.Measures);
            return new TsFeatures(e.TimeSignature, e.Subdivisions, startsPerBar);
        }

        private static Result Fail(List<string> warnings, string reason)
        {
            warnings.Add(reason);
            return new Result(false, null, null, warnings);
        }

        private static string Name(PaletteDescriptor p) =>
            string.IsNullOrWhiteSpace(p.DisplayName) ? (p.Id ?? "(unnamed)") : p.DisplayName;

        private static string ListNames(List<PaletteDescriptor> list)
        {
            var names = new List<string>(list.Count);
            foreach (var p in list) names.Add($"'{Name(p)}'");
            return string.Join(", ", names);
        }
    }
}
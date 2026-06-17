using System;
using System.Collections.Generic;
using NUnit.Framework;

using MidiGenPlay.Composition;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CardPaletteIntentResolver"/> (CE-L1 B1).
    /// Pure inputs only — synthetic descriptors, no AssetDatabase — mirroring
    /// the CE-F1 PaletteSelectorTests discipline.
    /// </summary>
    public sealed class CardPaletteIntentResolverTests
    {
        private const int MinSubs = 4;

        private static PaletteDescriptor Palette(
            string id, string name, string notes, bool drum, params (TimeSignature ts, int subs, int measures, int onsets)[] entries)
        {
            var d = new PaletteDescriptor { Id = id, DisplayName = name, Notes = notes, IsDrumDomain = drum };
            foreach (var e in entries)
            {
                d.Entries.Add(new PaletteEntryDescriptor
                {
                    TimeSignature = e.ts,
                    Subdivisions = e.subs,
                    Measures = e.measures,
                    StructuralOnsets = e.onsets
                });
            }
            return d;
        }

        private static PaletteDescriptor SixEightFunk =>
            Palette("Assets/P/funk68.asset", "Funk 6/8", "halftime funk groove", true,
                (TimeSignature.SixEight, 4, 2, 8));

        private static PaletteDescriptor FourFourRock =>
            Palette("Assets/P/rock44.asset", "Rock 4/4", "straight backbeat", true,
                (TimeSignature.FourFour, 4, 2, 8));

        private static PaletteDescriptor SevenEightOdd =>
            Palette("Assets/P/odd78.asset", "Odd 7/8", "angular", true,
                (TimeSignature.SevenEight, 4, 2, 6));

        // -------------------------------------------------------------------
        // Tier A: an exact-TS palette must always win over near matches.
        // -------------------------------------------------------------------

        [Test]
        public void ExactTsPalette_AlwaysWins([Values(1, 7, 42, 9001)] int seed)
        {
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk, SevenEightOdd };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, TimeSignature.SixEight, null, MinSubs, new Random(seed));

            Assert.IsTrue(result.Success, string.Join("; ", result.Warnings));
            Assert.AreEqual(SixEightFunk.Id, result.ChosenId,
                "Tier A must select the only exact-6/8 palette regardless of seed");
        }

        [Test]
        public void NoExactTs_PicksNearest_AndWarns()
        {
            // Desire 9/8; offer 6/8 (compound, eighth beat unit) vs 4/4.
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, TimeSignature.NineEight, null, MinSubs, new Random(123));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(HasWarningContaining(result, "exact"), "should warn that no exact-TS palette exists");
        }

        // -------------------------------------------------------------------
        // Keyword pre-filter
        // -------------------------------------------------------------------

        [Test]
        public void KeywordFilter_NarrowsToMatch()
        {
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk };

            // "funk" only matches the 6/8 palette even though we ask for 4/4:
            // keywords narrow first, then meter ranks among survivors.
            var result = CardPaletteIntentResolver.Resolve(
                palettes, TimeSignature.FourFour, new[] { "funk" }, MinSubs, new Random(5));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SixEightFunk.Id, result.ChosenId);
        }

        [Test]
        public void KeywordFilter_MatchesNotes_CaseInsensitive()
        {
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, null, new[] { "BACKBEAT" }, MinSubs, new Random(5));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FourFourRock.Id, result.ChosenId, "keyword must match against Notes too, case-insensitively");
        }

        [Test]
        public void UnmatchedKeywords_HardFail_ListingAvailablePalettes()
        {
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, TimeSignature.FourFour, new[] { "bossa" }, MinSubs, new Random(5));

            Assert.IsFalse(result.Success, "no silent fallback: unmatched keywords must fail");
            Assert.IsNull(result.ChosenId);
            Assert.IsTrue(HasWarningContaining(result, "Funk 6/8"), "failure must list available palettes");
            Assert.IsTrue(HasWarningContaining(result, "Rock 4/4"));
        }

        [Test]
        public void AllKeywordsMustMatch()
        {
            var palettes = new List<PaletteDescriptor> { SixEightFunk };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, null, new[] { "funk", "samba" }, MinSubs, new Random(5));

            Assert.IsFalse(result.Success, "AND semantics: one unmatched keyword fails the palette");
        }

        // -------------------------------------------------------------------
        // Determinism + scan-order invariance
        // -------------------------------------------------------------------

        [Test]
        public void SameSeed_SamePick_NullTs()
        {
            var palettes = new List<PaletteDescriptor> { FourFourRock, SixEightFunk, SevenEightOdd };

            var a = CardPaletteIntentResolver.Resolve(palettes, null, null, MinSubs, new Random(99));
            var b = CardPaletteIntentResolver.Resolve(palettes, null, null, MinSubs, new Random(99));

            Assert.IsTrue(a.Success && b.Success);
            Assert.AreEqual(a.ChosenId, b.ChosenId);
        }

        [Test]
        public void InputOrder_DoesNotChangePick([Values(0, 3, 17, 256)] int seed)
        {
            var ordered = new List<PaletteDescriptor> { FourFourRock, SixEightFunk, SevenEightOdd };
            var shuffled = new List<PaletteDescriptor> { SevenEightOdd, FourFourRock, SixEightFunk };

            var a = CardPaletteIntentResolver.Resolve(ordered, null, null, MinSubs, new Random(seed));
            var b = CardPaletteIntentResolver.Resolve(shuffled, null, null, MinSubs, new Random(seed));

            Assert.AreEqual(a.ChosenId, b.ChosenId,
                "project-scan order must never change the pick (descriptors are sorted by Id)");
        }

        [Test]
        public void InputOrder_DoesNotChangePick_WithTs([Values(0, 3, 17)] int seed)
        {
            // Two exact 4/4 palettes: the weighted draw among them must be order-independent.
            var rockB = Palette("Assets/P/rock44b.asset", "Rock 4/4 B", "variation", true,
                (TimeSignature.FourFour, 4, 2, 8));

            var ordered = new List<PaletteDescriptor> { FourFourRock, rockB };
            var shuffled = new List<PaletteDescriptor> { rockB, FourFourRock };

            var a = CardPaletteIntentResolver.Resolve(ordered, TimeSignature.FourFour, null, MinSubs, new Random(seed));
            var b = CardPaletteIntentResolver.Resolve(shuffled, TimeSignature.FourFour, null, MinSubs, new Random(seed));

            Assert.AreEqual(a.ChosenId, b.ChosenId);
        }

        // -------------------------------------------------------------------
        // Sanitization / degenerate inputs
        // -------------------------------------------------------------------

        [Test]
        public void EmptyPaletteList_Fails()
        {
            var result = CardPaletteIntentResolver.Resolve(
                new List<PaletteDescriptor>(), TimeSignature.FourFour, null, MinSubs, new Random(1));

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void EntrylessPalette_IsSkippedWithWarning()
        {
            var empty = new PaletteDescriptor { Id = "Assets/P/empty.asset", DisplayName = "Empty", IsDrumDomain = true };
            var palettes = new List<PaletteDescriptor> { empty, FourFourRock };

            var result = CardPaletteIntentResolver.Resolve(
                palettes, TimeSignature.FourFour, null, MinSubs, new Random(1));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(FourFourRock.Id, result.ChosenId);
            Assert.IsTrue(HasWarningContaining(result, "no entries"));
        }

        // -------------------------------------------------------------------
        // Feature computation parity with CE-F1
        // -------------------------------------------------------------------

        [Test]
        public void DrumFeatures_UseCappedFoundationDensity()
        {
            // 16 kick onsets over 2 bars = 8/bar, but 4/4 groupCount caps at 2 (CE-F1 D-F1.5).
            var e = new PaletteEntryDescriptor
            {
                TimeSignature = TimeSignature.FourFour,
                Subdivisions = 4,
                Measures = 2,
                StructuralOnsets = 16
            };
            int groupCount = PaletteSelector.DefaultGroupingCount(TimeSignature.FourFour);

            var f = CardPaletteIntentResolver.FeaturesFor(e, isDrum: true, groupCountForDesiredTs: groupCount);

            Assert.AreEqual(groupCount, f.StartsPerBar, 0.0001f, "busy grooves must not be penalized (capped density)");
        }

        [Test]
        public void ChordFeatures_UseHarmonicRhythm()
        {
            var e = new PaletteEntryDescriptor
            {
                TimeSignature = TimeSignature.FourFour,
                Subdivisions = 4,
                Measures = 4,
                StructuralOnsets = 8
            };

            var f = CardPaletteIntentResolver.FeaturesFor(e, isDrum: false, groupCountForDesiredTs: 2);

            Assert.AreEqual(2f, f.StartsPerBar, 0.0001f, "chords: starts/bar = events / measures");
        }

        [Test]
        public void RepresentativeFeatures_PrefersExactEntry()
        {
            var mixed = Palette("Assets/P/mixed.asset", "Mixed", "", false,
                (TimeSignature.FourFour, 4, 4, 8),
                (TimeSignature.SixEight, 4, 2, 4));

            var f = CardPaletteIntentResolver.RepresentativeFeatures(
                mixed, TimeSignature.SixEight, MinSubs, out bool exact);

            Assert.IsTrue(exact);
            Assert.AreEqual(TimeSignature.SixEight, f.TimeSignature);
        }

        private static bool HasWarningContaining(CardPaletteIntentResolver.Result r, string fragment)
        {
            foreach (var w in r.Warnings)
                if (w != null && w.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }
    }
}

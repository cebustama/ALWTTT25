using NUnit.Framework;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CardLLMResponseHandler"/> (CE-L1 B2): the
    /// banned-asset-path guard, the out-of-alphabet guard, and deterministic
    /// palette-intent resolution. End-to-end via FromPayload (fence-extract →
    /// parse → guard → validate → resolve) with no network.
    /// </summary>
    public sealed class CardLLMResponseHandlerTests
    {
        private const int Seed = 1234;

        private static CardLLMResponseHandler.Outcome Run(string json, int seed = Seed)
            => CardLLMResponseHandler.FromPayload(
                json, CardLLMPromptBuilderTests.MakeVocabulary(), seed);

        private static bool HasWarningContaining(CardLLMResponseHandler.Outcome o, string fragment)
        {
            foreach (var w in o.DisplayWarnings)
                if (w != null && w.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        // -------------------------------------------------------------------
        // Happy paths
        // -------------------------------------------------------------------

        [Test]
        public void MinimalValidCard_Stages()
        {
            var o = Run("{ \"kind\": \"Action\", \"id\": \"crd_min\" }");

            Assert.AreEqual(CardLLMResponseHandler.OutcomeKind.Staged, o.Kind,
                string.Join("; ", o.DisplayWarnings));
            Assert.AreEqual("crd_min", o.Dto.id);
            Assert.IsFalse(o.PaletteResolved);
        }

        [Test]
        public void FencedPayload_IsAcceptedByImport()
        {
            var o = Run("Model said:\n```json\n{ \"kind\": \"Action\", \"id\": \"crd_f\" }\n```");
            Assert.IsTrue(o.Success, string.Join("; ", o.DisplayWarnings));
        }

        [Test]
        public void FullCompositionCard_WithEffectsAndIntent_Stages_AndResolvesPalette()
        {
            string json =
                "{ \"kind\": \"Composition\", \"id\": \"crd_funky\"," +
                "  \"cardType\": \"CHR\", \"rarity\": \"Rare\", \"keywords\": [\"Exhaust\"]," +
                "  \"exhaustAfterPlay\": true," +
                "  \"effects\": [" +
                "    { \"type\": \"ApplyStatusEffect\", \"statusKey\": \"flow\", \"targetType\": \"Self\", \"stacksDelta\": 2 }," +
                "    { \"type\": \"DrawCards\", \"count\": 2 } ]," +
                "  \"composition\": {" +
                "    \"primaryKind\": \"Track\"," +
                "    \"trackAction\": { \"role\": \"Rhythm\" }," +
                "    \"modifierEffectNames\": [\"TempoUp10\"]," +
                "    \"palette\": { \"requested\": true, \"timeSignature\": \"SixEight\" } } }";

            var o = Run(json);

            Assert.AreEqual(CardLLMResponseHandler.OutcomeKind.Staged, o.Kind,
                string.Join("; ", o.DisplayWarnings));
            Assert.IsTrue(o.PaletteResolved);
            Assert.AreEqual("Assets/P/funk68.asset", o.ResolvedPaletteId);
            Assert.AreEqual("Funk 6/8", o.ResolvedPaletteDisplayName);
        }

        [Test]
        public void PaletteResolution_IsDeterministicPerSeed()
        {
            string json =
                "{ \"kind\": \"Composition\", \"id\": \"crd_p\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\" }," +
                "  \"palette\": { \"requested\": true } } }";

            var a = Run(json, seed: 7);
            var b = Run(json, seed: 7);

            Assert.IsTrue(a.Success && b.Success);
            Assert.AreEqual(a.ResolvedPaletteId, b.ResolvedPaletteId,
                "same payload + same seed must resolve the same palette (DoD determinism)");
        }

        // -------------------------------------------------------------------
        // Single-card rule + parse failures
        // -------------------------------------------------------------------

        [Test]
        public void BatchPayload_Fails()
        {
            var o = Run("{ \"cards\": [ { \"kind\": \"Action\", \"id\": \"a\" }, { \"kind\": \"Action\", \"id\": \"b\" } ] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "exactly one card"));
        }

        [Test]
        public void GarbagePayload_Fails()
        {
            var o = Run("not json at all");
            Assert.IsFalse(o.Success);
        }

        [Test]
        public void MissingId_Fails()
        {
            var o = Run("{ \"kind\": \"Action\" }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "id"));
        }

        [Test]
        public void MissingKind_Fails()
        {
            var o = Run("{ \"id\": \"crd_nokind\" }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "kind"));
        }

        // -------------------------------------------------------------------
        // Banned-asset-path guard
        // -------------------------------------------------------------------

        [Test]
        public void CardSpritePath_IsBanned()
        {
            var o = Run("{ \"kind\": \"Action\", \"id\": \"x\", \"cardSpritePath\": \"Assets/Sprites/a.png\" }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "cardSpritePath"));
        }

        [Test]
        public void StyleBundlePath_IsBanned()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\", \"styleBundle\": \"Assets/B/x.asset\" } } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "styleBundle"));
        }

        [Test]
        public void ModifierEffectPaths_AreBanned()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"modifierEffects\": [\"Assets/FX/Tempo.asset\"] } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "modifierEffects"));
        }

        [Test]
        public void PathShapedModifierName_IsBanned()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"modifierEffectNames\": [\"Assets/FX/TempoUp10.asset\"] } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "looks like an asset path"));
        }

        [Test]
        public void GuidShapedModifierName_IsBanned()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"modifierEffectNames\": [\"0123456789abcdef0123456789abcdef\"] } }");
            Assert.IsFalse(o.Success);
        }

        [Test]
        public void LegacyStatusActions_AreBanned()
        {
            var o = Run(
                "{ \"kind\": \"Action\", \"id\": \"x\"," +
                "  \"statusActions\": [ { \"statusKey\": \"flow\" } ] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "statusActions"));
        }

        // -------------------------------------------------------------------
        // Out-of-alphabet guard (the card D-L4.5 analog)
        // -------------------------------------------------------------------

        [Test]
        public void UnknownRarity_FailsNamingTokenAndField()
        {
            var o = Run("{ \"kind\": \"Action\", \"id\": \"x\", \"rarity\": \"Mythic\" }");
            Assert.IsFalse(o.Success,
                "staging would silently keep the default; the guard must block instead");
            Assert.IsTrue(HasWarningContaining(o, "Mythic"));
            Assert.IsTrue(HasWarningContaining(o, "rarity"));
        }

        [Test]
        public void UnknownStatusKey_Fails()
        {
            var o = Run(
                "{ \"kind\": \"Action\", \"id\": \"x\"," +
                "  \"effects\": [ { \"type\": \"ApplyStatusEffect\", \"statusKey\": \"hyperfocus\" } ] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "hyperfocus"));
        }

        [Test]
        public void NumericEffectId_IsNotAcceptedFromGeneration()
        {
            var o = Run(
                "{ \"kind\": \"Action\", \"id\": \"x\"," +
                "  \"effects\": [ { \"type\": \"ApplyStatusEffect\", \"effectId\": 3 } ] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "statusKey"));
        }

        [Test]
        public void UnknownEffectType_Fails()
        {
            var o = Run(
                "{ \"kind\": \"Action\", \"id\": \"x\"," +
                "  \"effects\": [ { \"type\": \"SummonDragon\" } ] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "SummonDragon"));
        }

        [Test]
        public void EnumCasing_IsForgiven()
        {
            var o = Run("{ \"kind\": \"action\", \"id\": \"x\", \"rarity\": \"rare\" }");
            Assert.IsTrue(o.Success, string.Join("; ", o.DisplayWarnings));
        }

        [Test]
        public void MultipleViolations_AreAllReported()
        {
            var o = Run(
                "{ \"kind\": \"Action\", \"id\": \"x\", \"rarity\": \"Mythic\"," +
                "  \"cardSpritePath\": \"Assets/a.png\"," +
                "  \"keywords\": [\"Banana\"] }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "Mythic"));
            Assert.IsTrue(HasWarningContaining(o, "cardSpritePath"));
            Assert.IsTrue(HasWarningContaining(o, "Banana"));
        }

        [Test]
        public void UnknownEntryFlag_Fails_ButRewardSynonymPasses()
        {
            var bad = Run("{ \"kind\": \"Action\", \"id\": \"x\", \"entry\": { \"flags\": \"UnlockedByDefault,Banana\" } }");
            Assert.IsFalse(bad.Success);

            var ok = Run("{ \"kind\": \"Action\", \"id\": \"x\", \"entry\": { \"flags\": \"UnlockedByDefault,Rewards\" } }");
            Assert.IsTrue(ok.Success, string.Join("; ", ok.DisplayWarnings));
        }

        // -------------------------------------------------------------------
        // Palette intent edge cases
        // -------------------------------------------------------------------

        [Test]
        public void PaletteIntent_ForMelodyRole_Fails()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Melody\" }," +
                "  \"palette\": { \"requested\": true } } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "Melody"));
        }

        [Test]
        public void PaletteIntent_WithoutRole_Fails()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"palette\": { \"requested\": true } } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "role"));
        }

        [Test]
        public void PaletteIntent_UnknownTimeSignature_Fails()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\" }," +
                "  \"palette\": { \"requested\": true, \"timeSignature\": \"ElevenEight\" } } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "ElevenEight"));
        }

        [Test]
        public void PaletteIntent_UnmatchedKeywords_FailsWithAvailableList()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\" }," +
                "  \"palette\": { \"keywords\": [\"bossa\"] } } }");
            Assert.IsFalse(o.Success);
            Assert.IsTrue(HasWarningContaining(o, "Funk 6/8"), "failure must list available palettes");
        }

        [Test]
        public void EmptyPaletteObject_WithoutRequestedFlagOrContent_IsNotAnIntent()
        {
            // JsonUtility default-constructs absent nested objects; an empty palette
            // object must therefore NOT attach a palette.
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\" }," +
                "  \"palette\": { } } }");
            Assert.IsTrue(o.Success, string.Join("; ", o.DisplayWarnings));
            Assert.IsFalse(o.PaletteResolved);
        }

        [Test]
        public void KeywordsAlone_CountAsIntent()
        {
            var o = Run(
                "{ \"kind\": \"Composition\", \"id\": \"x\"," +
                "  \"composition\": { \"trackAction\": { \"role\": \"Rhythm\" }," +
                "  \"palette\": { \"keywords\": [\"funk\"] } } }");
            Assert.IsTrue(o.Success, string.Join("; ", o.DisplayWarnings));
            Assert.IsTrue(o.PaletteResolved);
        }
    }
}

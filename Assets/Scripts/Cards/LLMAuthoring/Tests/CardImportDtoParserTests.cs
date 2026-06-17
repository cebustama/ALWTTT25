using NUnit.Framework;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CardImportDtoParser"/> (CE-L1 B1). The
    /// parser logic was moved verbatim from the window; these tests pin its
    /// behavior now that both the legacy JSON box and the LLM path depend on it.
    /// </summary>
    public sealed class CardImportDtoParserTests
    {
        [Test]
        public void SingleCardObject_Parses()
        {
            string json = "{\"kind\":\"Composition\",\"id\":\"crd_test\",\"inspirationCost\":2}";

            bool ok = CardImportDtoParser.TryParse(json, out var dtos, out var error);

            Assert.IsTrue(ok, error);
            Assert.AreEqual(1, dtos.Length);
            Assert.AreEqual("crd_test", dtos[0].id);
            Assert.AreEqual("Composition", dtos[0].kind);
            Assert.AreEqual(2, dtos[0].inspirationCost);
        }

        [Test]
        public void BatchWrapper_ParsesAllCards()
        {
            string json =
                "{\"cards\":[{\"kind\":\"Action\",\"id\":\"a1\"},{\"kind\":\"Composition\",\"id\":\"c1\"}]}";

            bool ok = CardImportDtoParser.TryParse(json, out var dtos, out var error);

            Assert.IsTrue(ok, error);
            Assert.AreEqual(2, dtos.Length);
            Assert.AreEqual("a1", dtos[0].id);
            Assert.AreEqual("c1", dtos[1].id);
        }

        [Test]
        public void BatchWrapper_DefaultEntry_MergesIntoCardsWithoutEntry()
        {
            string json =
                "{\"defaultEntry\":{\"flags\":\"UnlockedByDefault\",\"starterCopies\":3}," +
                "\"cards\":[" +
                "{\"id\":\"noEntry\"}," +
                "{\"id\":\"ownEntry\",\"entry\":{\"flags\":\"StarterDeck\",\"starterCopies\":1}}" +
                "]}";

            bool ok = CardImportDtoParser.TryParse(json, out var dtos, out var error);

            Assert.IsTrue(ok, error);
            Assert.IsNotNull(dtos[0].entry, "defaultEntry should be merged into a card with no entry");
            Assert.AreEqual("UnlockedByDefault", dtos[0].entry.flags);
            Assert.AreEqual("StarterDeck", dtos[1].entry.flags, "a card's own entry must win over defaultEntry");
        }

        [Test]
        public void RawArrayRoot_IsRejectedWithGuidance()
        {
            bool ok = CardImportDtoParser.TryParse("[{\"id\":\"x\"}]", out var dtos, out var error);

            Assert.IsFalse(ok);
            Assert.IsNull(dtos);
            StringAssert.Contains("Batch wrapper", error);
        }

        [Test]
        public void EmptyInput_Fails()
        {
            Assert.IsFalse(CardImportDtoParser.TryParse("   ", out _, out var error));
            StringAssert.Contains("empty", error);
        }

        [Test]
        public void PaletteIntent_RoundTripsThroughJson()
        {
            string json =
                "{\"kind\":\"Composition\",\"id\":\"crd_pal\"," +
                "\"composition\":{\"primaryKind\":\"Track\"," +
                "\"trackAction\":{\"role\":\"Rhythm\"}," +
                "\"palette\":{\"timeSignature\":\"SixEight\",\"keywords\":[\"funk\",\"halftime\"]}}}";

            bool ok = CardImportDtoParser.TryParse(json, out var dtos, out var error);

            Assert.IsTrue(ok, error);
            var pal = dtos[0].composition.palette;
            Assert.IsNotNull(pal);
            Assert.AreEqual("SixEight", pal.timeSignature);
            Assert.AreEqual(2, pal.keywords.Length);
            Assert.AreEqual("funk", pal.keywords[0]);
        }
    }
}

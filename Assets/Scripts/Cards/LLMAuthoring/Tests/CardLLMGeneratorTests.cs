using NUnit.Framework;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CardLLMGenerator"/> (CE-L1 B2), using the
    /// consumer-side <see cref="FakeLLMClient"/> on the real
    /// PromptExecutionHelper call path.
    /// </summary>
    public sealed class CardLLMGeneratorTests
    {
        private const string GoodResponse =
            "Here is your card.\n```json\n{ \"kind\": \"Composition\", \"id\": \"crd_x\" }\n```\nEnjoy!";

        [Test]
        public void Generate_HappyPath_ExtractsJson_AndFakeIsActuallyCalled()
        {
            var fake = new FakeLLMClient(GoodResponse, inputTokens: 321, outputTokens: 45);
            var vocab = CardLLMPromptBuilderTests.MakeVocabulary();

            var r = CardLLMGenerator.GenerateAsync(
                fake, vocab, new CardLLMPromptBuilder.Input("a card")).GetAwaiter().GetResult();

            Assert.IsTrue(fake.WasCalled, "the double must sit on the real call path");
            Assert.IsTrue(r.Success, r.FailureReason);
            StringAssert.Contains("\"id\": \"crd_x\"", r.ExtractedJson);
            Assert.AreEqual(321, r.InputTokens);
            Assert.AreEqual(45, r.OutputTokens);
            Assert.AreEqual(GoodResponse, r.RawResponse);
        }

        [Test]
        public void Generate_PassesSystemAndUserPrompts()
        {
            var fake = new FakeLLMClient(GoodResponse);
            var vocab = CardLLMPromptBuilderTests.MakeVocabulary();

            CardLLMGenerator.GenerateAsync(
                fake, vocab, new CardLLMPromptBuilder.Input("a very specific brief")).GetAwaiter().GetResult();

            StringAssert.Contains("a very specific brief", fake.LastPrompt);
            StringAssert.Contains("EXACTLY ONE card", fake.LastInstructions);
        }

        [Test]
        public void OverBudget_FailsBeforeNetwork_FakeNeverCalled()
        {
            var fake = new FakeLLMClient(GoodResponse);
            var vocab = CardLLMPromptBuilderTests.MakeVocabulary();

            var r = CardLLMGenerator.GenerateAsync(
                fake, vocab, new CardLLMPromptBuilder.Input("a card", maxCharBudget: 10))
                .GetAwaiter().GetResult();

            Assert.IsFalse(r.Success);
            Assert.IsFalse(fake.WasCalled, "cost cap is pre-network (SSoT §3.6): nothing may be sent");
        }

        [Test]
        public void ProseOnlyResponse_FailsWithLocateMessage_RawPreserved()
        {
            var fake = new FakeLLMClient("Sorry, I cannot produce that card.");
            var vocab = CardLLMPromptBuilderTests.MakeVocabulary();

            var r = CardLLMGenerator.GenerateAsync(
                fake, vocab, new CardLLMPromptBuilder.Input("a card")).GetAwaiter().GetResult();

            Assert.IsFalse(r.Success);
            StringAssert.Contains("locate", r.FailureReason);
            StringAssert.Contains("Sorry", r.RawResponse);
        }

        [Test]
        public void NullClient_Fails()
        {
            var r = CardLLMGenerator.GenerateAsync(
                null, CardLLMPromptBuilderTests.MakeVocabulary(),
                new CardLLMPromptBuilder.Input("a card")).GetAwaiter().GetResult();
            Assert.IsFalse(r.Success);
        }

        // -------------------------------------------------------------------
        // Extraction matrix (CRLF-safe per SSoT §3.4)
        // -------------------------------------------------------------------

        [Test]
        public void Extract_FencedJsonTag()
        {
            string json = CardLLMGenerator.ExtractJsonBlock("```json\n{ \"id\": \"a\" }\n```");
            Assert.AreEqual("{ \"id\": \"a\" }", json);
        }

        [Test]
        public void Extract_FencedNoTag()
        {
            string json = CardLLMGenerator.ExtractJsonBlock("```\n{ \"id\": \"a\" }\n```");
            Assert.AreEqual("{ \"id\": \"a\" }", json);
        }

        [Test]
        public void Extract_CrlfFences()
        {
            string json = CardLLMGenerator.ExtractJsonBlock("```json\r\n{ \"id\": \"a\" }\r\n```");
            Assert.AreEqual("{ \"id\": \"a\" }", json);
        }

        [Test]
        public void Extract_NestedBraces_TakesFullObject()
        {
            string json = CardLLMGenerator.ExtractJsonBlock(
                "```json\n{ \"composition\": { \"primaryKind\": \"Track\" } }\n```");
            StringAssert.Contains("\"primaryKind\": \"Track\" } }", json);
        }

        [Test]
        public void Extract_BareObject()
        {
            string json = CardLLMGenerator.ExtractJsonBlock("  { \"id\": \"a\" }  ");
            Assert.AreEqual("{ \"id\": \"a\" }", json);
        }

        [Test]
        public void Extract_ObjectEmbeddedInProse_LastResortSlice()
        {
            string json = CardLLMGenerator.ExtractJsonBlock("Sure! { \"id\": \"a\" } Hope it helps.");
            Assert.AreEqual("{ \"id\": \"a\" }", json);
        }

        [Test]
        public void Extract_NoJson_ReturnsNull()
        {
            Assert.IsNull(CardLLMGenerator.ExtractJsonBlock("no json here"));
            Assert.IsNull(CardLLMGenerator.ExtractJsonBlock("   "));
            Assert.IsNull(CardLLMGenerator.ExtractJsonBlock(null));
        }
    }
}

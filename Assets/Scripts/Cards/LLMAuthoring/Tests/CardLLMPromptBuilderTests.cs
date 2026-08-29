using System.Collections.Generic;
using NUnit.Framework;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>EditMode tests for <see cref="CardLLMPromptBuilder"/> (CE-L1 B2).</summary>
    public sealed class CardLLMPromptBuilderTests
    {
        internal static CardLLMVocabulary MakeVocabulary()
        {
            return new CardLLMVocabulary
            {
                PerformerRules = new[] { "FixedMusicianType", "AnyMusician" },
                MusicianTypes = new[] { "None", "Cantante", "Conito" },
                CardTypes = new[] { "CHR", "PRC" },
                Rarities = new[] { "Common", "Rare" },
                AudioTypes = new[] { "Button", "Chord" },
                SpecialKeywords = new[] { "Exhaust", "Consume" },
                ActionTargetTypes = new[] { "Self", "Musician", "AllAudienceCharacters" },
                ActionTimings = new[] { "Always", "OnPlay" },
                TrackRoles = new[] { "Backing", "Melody", "Harmony", "Rhythm", "Bassline" },
                PrimaryKinds = new[] { "None", "Track", "Part" },
                PartActionKinds = new[] { "None", "CreatePart", "MarkSolo", "Custom" },
                AcquisitionFlags = new[] { "UnlockedByDefault", "StarterDeck", "RewardPool" },
                TimeSignatures = new[] { "FourFour", "SixEight", "SevenEight" },
                StatusKeys = new[] { "flow", "composure", "earworm" },
                ModifierEffectNames = new[] { "TempoUp10", "MeterTo68" },
                RhythmPalettes = new List<PaletteDescriptor>
                {
                    Palette("Assets/P/funk68.asset", "Funk 6/8", true, TimeSignature.SixEight)
                },
                BackingPalettes = new List<PaletteDescriptor>
                {
                    Palette("Assets/P/vamp.asset", "Modal Vamp", false, TimeSignature.FourFour)
                }
            };
        }

        private static PaletteDescriptor Palette(string id, string name, bool drum, TimeSignature ts)
        {
            var d = new PaletteDescriptor { Id = id, DisplayName = name, IsDrumDomain = drum };
            d.Entries.Add(new PaletteEntryDescriptor
            {
                TimeSignature = ts,
                Subdivisions = 4,
                Measures = 2,
                StructuralOnsets = 4
            });
            return d;
        }

        [Test]
        public void NullVocabulary_Fails()
        {
            var r = CardLLMPromptBuilder.Build(null, new CardLLMPromptBuilder.Input("a card"));
            Assert.IsFalse(r.Success);
        }

        [Test]
        public void EmptyBrief_Fails()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(), new CardLLMPromptBuilder.Input("   "));
            Assert.IsFalse(r.Success);
        }

        [Test]
        public void OverBudget_FailsPreNetwork_NamingBothNumbers()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(),
                new CardLLMPromptBuilder.Input("a card", maxCharBudget: 50));

            Assert.IsFalse(r.Success);
            StringAssert.Contains("50", r.FailureReason);
            Assert.IsNull(r.SystemPrompt, "an over-budget build must not hand back prompts");
        }

        [Test]
        public void ZeroBudget_MeansNoCap()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(),
                new CardLLMPromptBuilder.Input("a card", maxCharBudget: 0));
            Assert.IsTrue(r.Success, r.FailureReason);
        }

        [Test]
        public void SystemPrompt_DeclaresAlphabetsContractAndBans()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(), new CardLLMPromptBuilder.Input("a card"));
            Assert.IsTrue(r.Success, r.FailureReason);
            string s = r.SystemPrompt;

            // Output contract
            StringAssert.Contains("EXACTLY ONE card", s);
            StringAssert.Contains("```json", s);

            // Alphabets surfaced
            StringAssert.Contains("FixedMusicianType | AnyMusician", s);
            StringAssert.Contains("flow", s);
            StringAssert.Contains("TempoUp10", s);
            StringAssert.Contains("Funk 6/8", s);
            StringAssert.Contains("Modal Vamp", s);
            StringAssert.Contains("SixEight", s);

            // Banned fields named
            StringAssert.Contains("cardSpritePath", s);
            StringAssert.Contains("styleBundle", s);
            StringAssert.Contains("modifierEffects,", s);
            StringAssert.Contains("statusActions", s);

            // Palette intent contract
            StringAssert.Contains("\"requested\": true", s);
        }

        [Test]
        public void UserPrompt_CarriesBriefAndHints()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(),
                new CardLLMPromptBuilder.Input(
                    "a Rhythm card that adds 2 Flow and draws 2 cards",
                    kindHint: "Composition", roleHint: "Rhythm", defaultMusician: "Conito"));

            Assert.IsTrue(r.Success, r.FailureReason);
            StringAssert.Contains("adds 2 Flow", r.UserPrompt);
            StringAssert.Contains("Composition", r.UserPrompt);
            StringAssert.Contains("Rhythm", r.UserPrompt);
            StringAssert.Contains("Conito", r.UserPrompt);
        }

        /// <summary>
        /// The EFFECTS block is hand-maintained while the stage-1 alphabets self-update
        /// from Enum.GetNames(), so it silently ages behind the importer — it did: six
        /// entries vs seven discriminators, R5-d through R5-e. The list below mirrors
        /// CardEditorWindow.JsonImport's discriminator set; the test assembly cannot
        /// reference the editor assembly, so this stays a hand-copied contract — but it
        /// now fails loudly instead of yielding a generator that quietly cannot emit an
        /// effect the importer accepts.
        /// </summary>
        [Test]
        public void SystemPrompt_DeclaresEveryImporterEffectDiscriminator()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(), new CardLLMPromptBuilder.Input("a card"));
            Assert.IsTrue(r.Success, r.FailureReason);

            string[] discriminators =
            {
                "ApplyStatusEffect", "DrawCards", "ModifyVibe", "ModifyStress",
                "AddInspirationPerLoop", "RevealPreferences", "GrantBonusLoop"
            };

            foreach (var d in discriminators)
                StringAssert.Contains("\"type\": \"" + d + "\"", r.SystemPrompt,
                    $"the prompt does not declare the '{d}' effect the importer accepts");
        }

        [Test]
        public void SystemPrompt_DeclaresResourceCostPair()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(), new CardLLMPromptBuilder.Input("a card"));
            Assert.IsTrue(r.Success, r.FailureReason);
            StringAssert.Contains("resourceCostStatusKey", r.SystemPrompt);
            StringAssert.Contains("resourceCostAmount", r.SystemPrompt);
        }

        [Test]
        public void TotalCharCount_IsSumOfPrompts()
        {
            var r = CardLLMPromptBuilder.Build(MakeVocabulary(), new CardLLMPromptBuilder.Input("a card"));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(r.SystemPrompt.Length + r.UserPrompt.Length, r.TotalCharCount);
        }
    }
}
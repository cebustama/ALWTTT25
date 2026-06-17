using System.Collections.Generic;
using NUnit.Framework;

namespace ALWTTT.Cards.LLMAuthoring.Tests
{
    /// <summary>EditMode tests for <see cref="CardLLMFieldPlan"/> (CE-L1 B2).</summary>
    public sealed class CardLLMFieldPlanTests
    {
        private static CardLLMResponseHandler.Outcome Staged(
            string id, bool palette = false, string palId = null, string palName = null)
        {
            var dto = new CardJsonImport { id = id, kind = "Composition" };
            return new CardLLMResponseHandler.Outcome(
                CardLLMResponseHandler.OutcomeKind.Staged, dto,
                palette, palId, palName, new List<string>(), 10, 20);
        }

        private static CardLLMResponseHandler.Outcome Failed() =>
            new CardLLMResponseHandler.Outcome(
                CardLLMResponseHandler.OutcomeKind.Failed, null,
                false, null, null, new List<string> { "boom" }, 0, 0);

        [Test]
        public void StagedOutcome_PlansStage_NonErrorStatus()
        {
            var plan = CardLLMFieldPlan.From(Staged("crd_a"));

            Assert.IsTrue(plan.StageCard);
            Assert.AreEqual("crd_a", plan.Dto.id);
            Assert.IsFalse(plan.AssignPalette);
            Assert.IsFalse(plan.StatusIsError);
            StringAssert.Contains("crd_a", plan.StatusMessage);
            StringAssert.Contains("Save", plan.StatusMessage);
        }

        [Test]
        public void StagedWithPalette_PlansAssignment_AndNamesPaletteInStatus()
        {
            var plan = CardLLMFieldPlan.From(
                Staged("crd_b", palette: true, palId: "Assets/P/funk.asset", palName: "Funk 6/8"));

            Assert.IsTrue(plan.StageCard);
            Assert.IsTrue(plan.AssignPalette);
            Assert.AreEqual("Assets/P/funk.asset", plan.PaletteId);
            StringAssert.Contains("Funk 6/8", plan.StatusMessage);
        }

        [Test]
        public void FailedOutcome_PlansNothing_ErrorStatus()
        {
            var plan = CardLLMFieldPlan.From(Failed());

            Assert.IsFalse(plan.StageCard);
            Assert.IsNull(plan.Dto);
            Assert.IsFalse(plan.AssignPalette);
            Assert.IsTrue(plan.StatusIsError);
            StringAssert.Contains("Nothing staged", plan.StatusMessage);
        }
    }
}

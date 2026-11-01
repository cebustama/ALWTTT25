using ALWTTT.Managers;
using NUnit.Framework;

namespace ALWTTT.Tests
{
    /// <summary>
    /// [COH-1] EditMode tests for <see cref="GigLossOutcome.Resolve"/>
    /// (D-DIAG-1=A: cohesion 0 is the only defeat). First test under
    /// Assets/Tests/Editor (D-DIAG-3=C: no asmdef, Assembly-CSharp-Editor).
    /// </summary>
    public sealed class GigLossOutcomeTests
    {
        [Test]
        public void MapRun_NormalLoss_ContinuesWithPenalty()
        {
            var o = GigLossOutcome.Resolve(cohesionBefore: 3, penalty: 1, mapRun: true, finalEncounter: false);
            Assert.IsFalse(o.RunOver);
            Assert.AreEqual(2, o.CohesionAfter);
            Assert.AreEqual(GigLossOutcome.ContinueLabel, o.ButtonLabel);
            StringAssert.Contains("Cohesión 3 → 2", o.Body);
        }

        [Test]
        public void MapRun_PenaltyReachesZero_EndsRun()
        {
            var o = GigLossOutcome.Resolve(1, 2, true, false);
            Assert.IsTrue(o.RunOver);
            Assert.AreEqual(0, o.CohesionAfter);
            Assert.AreEqual(GigLossOutcome.EndOfTourLabel, o.ButtonLabel);
        }

        [Test]
        public void MapRun_Collapse_EndsRun()
        {
            var o = GigLossOutcome.Resolve(0, 1, true, false);
            Assert.IsTrue(o.RunOver);
            Assert.AreEqual("La banda se separa", o.Title);
        }

        [Test]
        public void MapRun_QueenLost_EndsRunEvenWithCohesionLeft()
        {
            var o = GigLossOutcome.Resolve(5, 1, true, true);
            Assert.IsTrue(o.RunOver);
            Assert.AreEqual(4, o.CohesionAfter);
            Assert.AreEqual(GigLossOutcome.EndOfTourLabel, o.ButtonLabel);
            StringAssert.DoesNotContain("Cohesión", o.Body);
        }

        [Test]
        public void GigSetup_NeverEndsRun_KeepsPrefabLabels()
        {
            // GigSetup sets IsFinalEncounter = true (A6 patch); still not a run end.
            var o = GigLossOutcome.Resolve(1, 3, false, true);
            Assert.IsFalse(o.RunOver);
            Assert.IsNull(o.ButtonLabel);
            Assert.IsFalse(GigLossOutcome.Resolve(0, 1, false, false).RunOver);
        }

        [Test]
        public void NegativeInputs_AreClamped()
        {
            var o = GigLossOutcome.Resolve(3, -2, true, false);
            Assert.AreEqual(3, o.CohesionAfter);
        }
    }
}

using System.Collections;
using System.Collections.Generic;

namespace ALWTTT.Generation
{
    public class BuildSpinesStage : ISectorGenStage
    {
        public string Name => "Build Forward Spines";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            if (ctx.Data.ensureTwoPaths) BuildTwo(ctx);
            else BuildOne(ctx);
            yield break;
        }

        private void BuildTwo(SectorGraphContext ctx)
        {
            int cols = ctx.Data.columns;
            int currentA = ctx.State.StartNodeId;
            int currentB = ctx.State.StartNodeId;

            for (int c = 0; c < cols - 1; c++)
            {
                var nextInCol = SectorGenUtils.GetNodesInColumnFromBuckets(ctx.Buckets, c + 1);
                if (nextInCol.Count == 0) continue;

                int pickA = SectorGenUtils.PickNextWithVerticalBias(
                    ctx.State, currentA, nextInCol, ctx.Rng, ctx.Data.forwardVerticalBias);

                int pickB;
                if (nextInCol.Count > 1)
                {
                    var tmp = new List<int>(nextInCol);
                    tmp.Remove(pickA);
                    pickB = SectorGenUtils.PickNextWithVerticalBias(
                        ctx.State, currentB, tmp, ctx.Rng, ctx.Data.forwardVerticalBias);
                }
                else pickB = pickA;

                SectorGenUtils.TryLinkBothWays(ctx.State, currentA, pickA, ctx.Data.maxDegreePerNode);
                SectorGenUtils.TryLinkBothWays(ctx.State, currentB, pickB, ctx.Data.maxDegreePerNode);

                currentA = pickA;
                currentB = pickB;
            }
        }

        private void BuildOne(SectorGraphContext ctx)
        {
            int cols = ctx.Data.columns;
            int current = ctx.State.StartNodeId;

            for (int c = 0; c < cols - 1; c++)
            {
                var nextInCol = SectorGenUtils.GetNodesInColumnFromBuckets(ctx.Buckets, c + 1);
                if (nextInCol.Count == 0) continue;

                int pick = SectorGenUtils.PickNextWithVerticalBias(
                    ctx.State, current, nextInCol, ctx.Rng, ctx.Data.forwardVerticalBias);

                SectorGenUtils.TryLinkBothWays(ctx.State, current, pick, ctx.Data.maxDegreePerNode);
                current = pick;
            }
        }
    }
}

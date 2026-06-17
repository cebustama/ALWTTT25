using System.Collections;
using UnityEngine;

namespace ALWTTT.Generation
{
    public class CrossLinksStage : ISectorGenStage
    {
        public string Name => "Add Cross Links";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            int cols = ctx.Data.columns;

            for (int c = 0; c < cols - 1; c++)
            {
                var a = SectorGenUtils.GetNodesInColumnFromBuckets(ctx.Buckets, c);
                var b = SectorGenUtils.GetNodesInColumnFromBuckets(ctx.Buckets, c + 1);

                foreach (var na in a)
                    foreach (var nb in b)
                    {
                        var an = ctx.State.GetNode(na);
                        var bn = ctx.State.GetNode(nb);

                        float dy = Mathf.Abs(bn.Position.y - an.Position.y);
                        if (ctx.Data.maxCrossLinkDeltaY > 0f && dy > ctx.Data.maxCrossLinkDeltaY) 
                            continue;

                        float scaledP = ctx.Data.linkProbability;
                        if (ctx.Data.crossLinkVerticalBias > 0f)
                            scaledP *= 1f / (1f + ctx.Data.crossLinkVerticalBias * dy);

                        if (ctx.Rng.NextDouble() <= scaledP)
                            SectorGenUtils.TryLinkBothWays(
                                ctx.State, na, nb, ctx.Data.maxDegreePerNode);
                    }
            }

            yield break;
        }
    }
}

using System.Collections;

namespace ALWTTT.Generation
{
    public class RepairGapsStage : ISectorGenStage
    {
        public string Name => "Repair Forward Gaps";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            SectorGenUtils.EnsureNoIsolatedForwardGaps(
                ctx.State, ctx.Data.columns, ctx.Rng, ctx.Buckets, ctx.Data.maxDegreePerNode);
            yield break;
        }
    }
}

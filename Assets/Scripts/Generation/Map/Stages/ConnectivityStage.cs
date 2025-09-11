using System.Collections;

namespace ALWTTT.Generation
{
    public class ConnectivityStage : ISectorGenStage
    {
        public string Name => "Connect Islands";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            if (ctx.Data.enforceSingleComponent)
            {
                SectorGenUtils.ConnectIslandsToMain(
                    ctx.State, ctx.Data.columns, ctx.Buckets, ctx.Data, ctx.Rng);
            }
            yield break;
        }
    }
}

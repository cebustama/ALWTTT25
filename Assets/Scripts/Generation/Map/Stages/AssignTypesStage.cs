using System.Collections;

namespace ALWTTT.Generation
{
    public class AssignTypesStage : ISectorGenStage
    {
        public string Name => "Assign Node Types";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            SectorGenUtils.AssignNodeTypes(ctx.State, ctx.Data, ctx.Rng);
            yield break;
        }
    }
}

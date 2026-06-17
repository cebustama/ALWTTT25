using System.Collections;
using UnityEngine;

namespace ALWTTT.Generation
{
    public class PickStartExitStage : ISectorGenStage
    {
        public string Name => "Pick Start/Exit";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            int cols = Mathf.Max(2, ctx.Data.columns);
            ctx.State.StartNodeId = SectorGenUtils.PickTopMost(ctx.State.Nodes, 0);
            ctx.State.ExitNodeId = SectorGenUtils.PickBottomMost(ctx.State.Nodes, cols - 1);
            ctx.State.CurrentNodeId = ctx.State.StartNodeId;
            ctx.State.GetNode(ctx.State.StartNodeId).Visited = true;
            yield break;
        }
    }
}

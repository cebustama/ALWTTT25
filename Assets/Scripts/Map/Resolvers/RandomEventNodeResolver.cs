using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System.Collections;

namespace ALWTTT.Map
{
    public class RandomEventNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.RandomEncounter;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            bool ok = true;
            yield return ctx.ShowRandomEvent(done => ok = done);

            if (ok)
            {
                // Effects already applied by the event controller (stub).
            }
        }
    }
}

using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System.Collections;

namespace ALWTTT.Map
{
    public class GigNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.Gig;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            bool won = false; int fans = 0;
            yield return ctx.RunGig(isBoss: false, (w, f) => { won = w; fans = f; });

            if (won)
            {
                GainFans(ctx, fans);
                // TODO: grant a card reward here
            }
            else
            {
                // Losing a regular gig has no special effect (stub)
            }
        }
    }
}

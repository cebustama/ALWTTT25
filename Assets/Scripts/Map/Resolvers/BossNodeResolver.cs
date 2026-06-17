using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System.Collections;

namespace ALWTTT.Map
{
    public class BossNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.Boss;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            bool won = false; int fans = 0;
            yield return ctx.RunGig(isBoss: true, (w, f) => { won = w; fans = f; });

            if (won)
            {
                GainFans(ctx, fans);
                ctx.TravelToNextSector(); // advance sector and regenerate map
            }
            else
            {
                ctx.GameOver();
            }
        }
    }
}

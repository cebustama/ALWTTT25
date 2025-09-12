using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System.Collections;

namespace ALWTTT.Map
{
    public class RecruitNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.Recruit;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            bool accepted = false;
            yield return ctx.ShowRecruit(a => accepted = a);

            if (accepted)
            {
                // TODO: add musician to band roster; log the non-chosen to rival list if applicable
                UnityEngine.Debug.Log("[Recruit] Musician joined (stub).");
            }
        }
    }
}

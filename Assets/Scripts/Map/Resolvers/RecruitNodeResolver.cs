using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Musicians;
using System.Collections;

namespace ALWTTT.Map
{
    public class RecruitNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.Recruit;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            MusicianCharacterData chosen = null;
            yield return ctx.ShowRecruit(m => chosen = m);

            if (chosen != null)
            {
                ctx.Persistent.AddMusicianToBand(chosen);
                node.Completed = true;

                ctx.RefreshHUD();
            }
        }
    }
}

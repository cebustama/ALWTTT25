using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Map
{
    public class RehearsalNodeResolver : NodeResolverBase
    {
        public override NodeType HandlesType => NodeType.Rehearsal;

        public override IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node)
        {
            string choice = null;
            yield return ctx.ShowRehearsalMenu(c => choice = c);

            switch (choice)
            {
                case "Compose":
                    // TODO: create a new song, add to band repertoire
                    Debug.Log("[Rehearsal] Compose: new song created (stub).");
                    break;
                case "Relax":
                    // TODO: heal stress on musicians
                    Debug.Log("[Rehearsal] Relax: stress healed (stub).");
                    break;
                case "BandTalk":
                    // TODO: solve conflicts / heal cohesion
                    Debug.Log("[Rehearsal] BandTalk: cohesion improved (stub).");
                    break;
                default:
                    Debug.Log("[Rehearsal] Default branch.");
                    break;
            }
        }
    }
}
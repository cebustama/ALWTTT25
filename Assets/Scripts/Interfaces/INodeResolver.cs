using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Map;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Interfaces
{
    /// <summary>Strategy for resolving a node interaction.</summary>
    public interface INodeResolver
    {
        NodeType HandlesType { get; }
        IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node);
    }

    /// <summary>Optional base with helpers.</summary>
    public abstract class NodeResolverBase : INodeResolver
    {
        public abstract NodeType HandlesType { get; }
        public abstract IEnumerator Resolve(NodeResolveContext ctx, SectorNodeState node);

        protected void GainFans(NodeResolveContext ctx, int amount)
        {
            ctx.Persistent.Fans += amount;
            ctx.Manager.RefreshHUD();
        }
    }
}

using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ALWTTT.Map
{
    public static class NodeResolverProcessor
    {
        private static readonly Dictionary<NodeType, INodeResolver> _dict = new();
        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            _dict.Clear();
            var types = Assembly.GetAssembly(typeof(NodeResolverBase))
                .GetTypes()
                .Where(t => typeof(INodeResolver).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var t in types)
            {
                if (Activator.CreateInstance(t) is INodeResolver r)
                    _dict[r.HandlesType] = r;
            }
            IsInitialized = true;
        }

        public static INodeResolver Get(NodeType type) => _dict[type];
    }
}
using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Enums;

namespace ALWTTT.Data
{
    [CreateAssetMenu(
        fileName = "NodeTypeDatabase",
        menuName = "ALWTTT/Map/Node Type Database",
        order = 1101)]
    public class NodeTypeDatabase : ScriptableObject
    {
        [SerializeField] private List<NodeTypeData> entries = new();

        private Dictionary<NodeType, NodeTypeData> _cache;

        public NodeTypeData Get(NodeType type)
        {
            if (_cache == null) BuildCache();
            return _cache.TryGetValue(type, out var data) ? data : null;
        }

        public IReadOnlyList<NodeTypeData> Entries => entries;

        private void BuildCache()
        {
            _cache = new Dictionary<NodeType, NodeTypeData>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                _cache[e.Type] = e; // last wins if duplicates
            }
        }

        private void OnValidate()
        {
            _cache = null; // rebuild next Get

            // Optional editor safety: ensure each enum has at most one entry
            // (not enforced, but this will help you spot duplicates in the list)
        }
    }
}

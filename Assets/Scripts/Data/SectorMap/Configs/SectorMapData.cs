using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(
        fileName = "SectorMapData",
        menuName = "ALWTTT/Map/Sector Map Data",
        order = 1000)]
    public class SectorMapData : ScriptableObject
    {
        [Header("Logic Layout")]
        [Min(2)] public int columns = 8;
        [Min(2)] public int rows = 7;

        [Tooltip("Total number of nodes for this sector (randomized between min and max).")]
        public Vector2Int totalNodesRange = new Vector2Int(18, 26);

        [Range(0f, 1f)]
        [Tooltip("Base probability to connect two valid neighboring nodes.")]
        public float linkProbability = 0.55f;

        [Tooltip("Local UI jitter to avoid perfectly overlapping/aligned nodes.")]
        public Vector2 positionJitter = new Vector2(0.35f, 0.25f);

        [Header("Connectivity requirements")]
        [Tooltip("Guarantee at least two distinct paths from Start to Exit.")]
        public bool ensureTwoPaths = true;

        [Tooltip("Target branching (heuristic, not a hard constraint).")]
        public Vector2Int branchesRange = new Vector2Int(2, 5);

        [Header("Node Type Data")]
        [Tooltip("Lookup of per-type metadata (title, description, color, sprite).")]
        [SerializeField] private NodeTypeDatabase nodeTypeDatabase;
        public NodeTypeDatabase NodeTypes => nodeTypeDatabase;

        [Header("Node Type Data")]
        [SerializeField]
        private List<NodeCountRule> nodeCountRules = new()
        {
            new NodeCountRule(NodeType.Rehearsal,        4,  8),
            new NodeCountRule(NodeType.Gig,              6, 10),
            new NodeCountRule(NodeType.RandomEvent,  3,  6),
            new NodeCountRule(NodeType.Recruit,          1,  3),
            new NodeCountRule(NodeType.Boss,             1,  1),
        };

        public IReadOnlyList<NodeCountRule> NodeCountRules => nodeCountRules;

        [Header("Sector end")]
        [Tooltip("If true, the exit node is forced to be a Boss at a Star Relay.")]
        public bool forceBossOnExit = true;

        [Header("Connectivity")]
        [Tooltip("Guarantee the graph is a single connected component by bridging islands to the main component.")]
        public bool enforceSingleComponent = true;

        [Header("Smoothing (layout heuristics)")]
        [Range(0f, 5f)]
        [Tooltip("Bias to prefer small |ΔY| when building guaranteed forward paths. 0 = no bias, higher = smoother.")]
        public float forwardVerticalBias = 1.25f;
        [Range(0f, 5f)]
        [Tooltip("Bias that reduces probability of random links with large |ΔY|. 0 = uniform, higher = penalize big jumps.")]
        public float crossLinkVerticalBias = 1.0f;
        [Range(0, 12)]
        [Tooltip("Maximum degree (number of links) per node. 0 or lower disables the cap.")]
        public int maxDegreePerNode = 4;
        [Range(0f, 10f)]
        [Tooltip("Hard limit: do not create random cross-links if |ΔY| exceeds this value. 0 disables.")]
        public float maxCrossLinkDeltaY = 0f;

        [Header("Seed / reproducibility (optional)")]
        [Tooltip("If >= 0 this seed will be used; if -1 a random seed is used at runtime.")]
        public int fixedSeed = -1;

        // ---------- Helper accessors ----------

        /// <summary>
        /// Get the (min,max) range for a given node type. Returns (0,0) if no rule exists.
        /// </summary>
        public (int min, int max) GetCountRange(NodeType type)
        {
            var rule = nodeCountRules.Find(r => r.type == type);
            return rule.IsValid ? (rule.min, rule.max) : (0, 0);
        }

        private void OnValidate()
        {
            // Keep ranges sane and consistent in the editor.
            totalNodesRange.x = Mathf.Max(1, totalNodesRange.x);
            totalNodesRange.y = Mathf.Max(totalNodesRange.x, totalNodesRange.y);

            branchesRange.x = Mathf.Max(0, branchesRange.x);
            branchesRange.y = Mathf.Max(branchesRange.x, branchesRange.y);

            for (int i = 0; i < nodeCountRules.Count; i++)
            {
                var r = nodeCountRules[i];
                r.min = Mathf.Max(0, r.min);
                r.max = Mathf.Max(r.min, r.max);
                nodeCountRules[i] = r;
            }

            forwardVerticalBias = Mathf.Max(0f, forwardVerticalBias);
            crossLinkVerticalBias = Mathf.Max(0f, crossLinkVerticalBias);
            maxDegreePerNode = Mathf.Max(0, maxDegreePerNode);
            maxCrossLinkDeltaY = Mathf.Max(0f, maxCrossLinkDeltaY);
        }

        public NodeTypeData GetNodeTypeData(NodeType type)
            => nodeTypeDatabase ? nodeTypeDatabase.Get(type) : null;

        public string GetNodeTypeTitle(NodeType type)
            => GetNodeTypeData(type)?.Title ?? type.ToString();

        public string GetNodeTypeDescription(NodeType type)
            => GetNodeTypeData(type)?.Description ?? string.Empty;

        [Serializable]
        public struct NodeCountRule
        {
            public NodeType type;
            public int min;
            public int max;

            public NodeCountRule(NodeType type, int min, int max)
            {
                this.type = type;
                this.min = min;
                this.max = Mathf.Max(min, max);
            }

            public bool IsValid => max >= min && min >= 0;
        }
    }
}
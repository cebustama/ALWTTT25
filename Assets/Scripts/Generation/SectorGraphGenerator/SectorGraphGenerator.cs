using System;
using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Enums;

namespace ALWTTT.Generation
{
    /// <summary>
    /// Pure generator service: takes SectorMapData + RNG and returns a SectorMapState.
    /// No Unity scene logic here (SRP). Keep it swappable behind an interface later (e.g., ISectorGraphGenerator).
    /// </summary>
    public class SectorGraphGenerator
    {
        // Reusable empty list for safe returns
        private static readonly List<int> s_empty = new();

        /// <summary>
        /// Build a SectorMapState for the given sectorId using the provided data and RNG.
        /// Column placement is FTL-like; connections guarantee forward flow; types come from SectorMapData rules.
        /// </summary>
        public SectorMapState Generate(
            int sectorId,
            SectorMapData data,
            System.Random rng = null)
        {
            rng ??= (data.fixedSeed >= 0)
                ? new System.Random(data.fixedSeed)
                : new System.Random(Guid.NewGuid().GetHashCode());

            // --- 1) Layout basics (grid + counts) ---
            int cols = Mathf.Max(2, data.columns);
            int rows = Mathf.Max(2, data.rows);
            int totalNodes = Mathf.Clamp(
                rng.Next(data.totalNodesRange.x, data.totalNodesRange.y + 1),
                cols, cols * rows // at least one per column
            );

            // For each column, decide which row indices will host nodes (at least 1 per column)
            var rowsPerColumn = AllocateNodesPerColumn(cols, rows, totalNodes, rng);

            // --- 2) Create nodes with ColumnIndex + jittered positions ---
            var state = new SectorMapState
            {
                SectorId = sectorId,
                Nodes = new List<SectorNodeState>()
            };

            int nextId = 0;
            for (int c = 0; c < cols; c++)
            {
                foreach (int r in rowsPerColumn[c])
                {
                    var node = new SectorNodeState
                    {
                        Id = nextId++,
                        ColumnIndex = c, // source of truth for column membership
                        Position = new Vector2(
                            c + RandomRange(rng, -data.positionJitter.x, data.positionJitter.x),
                            r + RandomRange(rng, -data.positionJitter.y, data.positionJitter.y)
                        ),
                        Type = NodeType.Rehearsal, // safe default; types will be assigned later
                        Visited = false,
                        Locked = false,
                        Links = new List<int>()
                    };
                    state.Nodes.Add(node);
                }
            }

            // Column buckets: column -> list of node ids (avoids scanning all nodes repeatedly)
            var buckets = BuildColumnBuckets(state, cols);

            // --- 3) Pick start/exit deterministically using ColumnIndex ---
            state.StartNodeId = PickTopMost(state.Nodes, 0);          // e.g., top-most on first column
            state.ExitNodeId = PickBottomMost(state.Nodes, cols - 1); // bottom-most on last column
            state.CurrentNodeId = state.StartNodeId;
            state.GetNode(state.StartNodeId).Visited = true;

            // --- 4) Connections (forward flow) ---
            // 4.1 two guaranteed spines if requested (or a single spine otherwise)
            if (data.ensureTwoPaths)
                BuildGuaranteedTwoPaths(state, cols, rng, buckets, data);
            else
                BuildSingleForwardPath(state, cols, rng, buckets, data);

            // 4.2 add random cross-links between adjacent columns
            AddRandomLinks(state, cols, data, rng, buckets);

            // 4.3 repair pass: ensure every column has at least one forward link
            EnsureNoIsolatedForwardGaps(state, cols, rng, buckets, data);

            // --- connectivity pass ---
            if (data.enforceSingleComponent)
                ConnectIslandsToMain(state, cols, buckets, data, rng);

            // --- 5) Node type assignment based on SectorMapData rules ---
            AssignNodeTypes(state, data, rng);

            return state;
        }

        #region Layout
        // ---------------------------------------------------------------------
        // Layout helpers
        // ---------------------------------------------------------------------

        /// <summary>
        /// Decide how many nodes each column gets and which row indices they occupy.
        /// Guarantees at least one row per column; rows are unique within a column.
        /// </summary>
        private List<List<int>> AllocateNodesPerColumn(
            int cols, int rows, int totalNodes, System.Random rng)
        {
            var result = new List<List<int>>(cols);
            for (int c = 0; c < cols; c++) result.Add(new List<int>());

            // First pass: guarantee 1 node per column.
            int remaining = totalNodes;
            for (int c = 0; c < cols; c++)
            {
                int r = rng.Next(0, rows);
                result[c].Add(r);
            }
            remaining -= cols;

            // Distribute the remaining nodes.
            while (remaining > 0)
            {
                int c = rng.Next(0, cols);
                if (result[c].Count < rows)
                {
                    // Pick an unused row in this column
                    int attempts = 0;
                    while (attempts++ < 16)
                    {
                        int r = rng.Next(0, rows);
                        if (!result[c].Contains(r))
                        {
                            result[c].Add(r);
                            remaining--;
                            break;
                        }
                    }
                }
            }

            // Sort rows in each column for nicer visuals (not required).
            for (int c = 0; c < cols; c++) result[c].Sort();

            return result;
        }

        private static float RandomRange(System.Random rng, float min, float max)
        {
            return (float)(min + (max - min) * rng.NextDouble());
        }

        // ---------------------------------------------------------------------
        // Column buckets
        // ---------------------------------------------------------------------

        /// <summary>
        /// Build an index: column -> list of node ids.
        /// Greatly reduces repeated scans across all nodes for adjacency and linking.
        /// </summary>
        private List<List<int>> BuildColumnBuckets(SectorMapState s, int cols)
        {
            var buckets = new List<List<int>>(cols);
            for (int c = 0; c < cols; c++) buckets.Add(new List<int>());

            foreach (var n in s.Nodes)
            {
                if (n.ColumnIndex >= 0 && n.ColumnIndex < cols)
                    buckets[n.ColumnIndex].Add(n.Id);
            }
            return buckets;
        }

        private List<int> GetNodesInColumnFromBuckets(List<List<int>> buckets, int column)
        {
            return (column >= 0 && column < buckets.Count) ? buckets[column] : s_empty;
        }

        // ---------------------------------------------------------------------
        // Start/Exit selectors
        // ---------------------------------------------------------------------

        private int PickTopMost(IReadOnlyList<SectorNodeState> nodes, int column)
        {
            int best = -1;
            float bestY = float.PositiveInfinity;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.ColumnIndex != column) continue;
                if (n.Position.y < bestY) { bestY = n.Position.y; best = n.Id; }
            }
            return best;
        }

        private int PickBottomMost(IReadOnlyList<SectorNodeState> nodes, int column)
        {
            int best = -1;
            float bestY = float.NegativeInfinity;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.ColumnIndex != column) continue;
                if (n.Position.y > bestY) { bestY = n.Position.y; best = n.Id; }
            }
            return best;
        }
        #endregion

        #region Graph Building
        // ---------------------------------------------------------------------
        // Graph building
        // ---------------------------------------------------------------------

        // Choose next node with vertical smoothing (prefer small |ΔY|)
        private int PickNextWithVerticalBias(
            SectorMapState s, int currentId, List<int> candidates, System.Random rng, float bias)
        {
            if (candidates == null || candidates.Count == 0) return -1;
            if (bias <= 0f) return candidates[rng.Next(0, candidates.Count)];

            var current = s.GetNode(currentId);
            int best = candidates[0];
            float bestScore = float.PositiveInfinity;

            for (int i = 0; i < candidates.Count; i++)
            {
                var next = s.GetNode(candidates[i]);
                float dy = Mathf.Abs(next.Position.y - current.Position.y);
                // smaller |ΔY| => smaller score; raise to "bias" to exaggerate differences
                float score = Mathf.Pow(dy, Mathf.Max(1f, bias));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidates[i];
                }
            }
            return best;
        }

        /// <summary>
        /// Build two forward "spines" from Start to Exit across columns, trying to use different nodes.
        /// </summary>
        private void BuildGuaranteedTwoPaths(
            SectorMapState s, int cols, System.Random rng, 
            List<List<int>> buckets, SectorMapData data)
        {
            int currentA = s.StartNodeId;
            int currentB = s.StartNodeId;

            for (int c = 0; c < cols - 1; c++)
            {
                var nextInCol = GetNodesInColumnFromBuckets(buckets, c + 1);
                if (nextInCol.Count == 0) continue;

                int pickA = PickNextWithVerticalBias(s, currentA, nextInCol, rng, data.forwardVerticalBias);

                int pickB;
                if (nextInCol.Count > 1)
                {
                    // Build a secondary path; remove pickA temporarily so we pick a different one if possible
                    var tmp = new List<int>(nextInCol);
                    tmp.Remove(pickA);
                    pickB = PickNextWithVerticalBias(s, currentB, tmp, rng, data.forwardVerticalBias);
                }
                else pickB = pickA;

                TryLinkBothWays(s, currentA, pickA, data.maxDegreePerNode);
                TryLinkBothWays(s, currentB, pickB, data.maxDegreePerNode);

                currentA = pickA;
                currentB = pickB;
            }
        }


        /// <summary>
        /// Build a single forward path (if ensureTwoPaths is disabled).
        /// </summary>
        private void BuildSingleForwardPath(
            SectorMapState s, int cols, System.Random rng, 
            List<List<int>> buckets, SectorMapData data)
        {
            int current = s.StartNodeId;
            for (int c = 0; c < cols - 1; c++)
            {
                var nextInCol = GetNodesInColumnFromBuckets(buckets, c + 1);
                if (nextInCol.Count == 0) continue;

                int pick = PickNextWithVerticalBias(s, current, nextInCol, rng, data.forwardVerticalBias);
                TryLinkBothWays(s, current, pick, data.maxDegreePerNode);
                current = pick;
            }
        }


        /// <summary>
        /// Random cross-links between adjacent columns based on linkProbability.
        /// </summary>
        private void AddRandomLinks(
            SectorMapState s, int cols, SectorMapData data, System.Random rng, 
            List<List<int>> buckets)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                var a = GetNodesInColumnFromBuckets(buckets, c);
                var b = GetNodesInColumnFromBuckets(buckets, c + 1);

                foreach (var na in a)
                    foreach (var nb in b)
                    {
                        var an = s.GetNode(na);
                        var bn = s.GetNode(nb);

                        float dy = Mathf.Abs(bn.Position.y - an.Position.y);

                        // Optional hard limit: skip links that jump too far vertically
                        if (data.maxCrossLinkDeltaY > 0f && dy > data.maxCrossLinkDeltaY) continue;

                        // Probability scaled by vertical bias (larger |ΔY| => lower probability)
                        float scaledP = data.linkProbability;
                        if (data.crossLinkVerticalBias > 0f)
                            scaledP *= 1f / (1f + data.crossLinkVerticalBias * dy);

                        if (rng.NextDouble() <= scaledP)
                            TryLinkBothWays(s, na, nb, data.maxDegreePerNode);
                    }
            }
        }

        /// <summary>
        /// Ensure each column has at least one forward connection to the next.
        /// </summary>
        private void EnsureNoIsolatedForwardGaps(
            SectorMapState s, int cols, System.Random rng,
            List<List<int>> buckets, SectorMapData data)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                var a = GetNodesInColumnFromBuckets(buckets, c);
                var b = GetNodesInColumnFromBuckets(buckets, c + 1);

                bool hasAny = false;
                foreach (var na in a)
                {
                    var links = s.GetNode(na).Links;
                    for (int i = 0; i < links.Count; i++)
                    {
                        if (s.GetNode(links[i]).ColumnIndex == c + 1)
                        {
                            hasAny = true;
                            break;
                        }
                    }
                    if (hasAny) break;
                }

                if (!hasAny && a.Count > 0 && b.Count > 0)
                {
                    int na = a[rng.Next(0, a.Count)];
                    int nb = b[rng.Next(0, b.Count)];
                    TryLinkBothWays(s, na, nb, data.maxDegreePerNode);
                }
            }
        }

        private bool TryLinkBothWays(SectorMapState s, int aId, int bId, int maxDegree)
        {
            var a = s.GetNode(aId);
            var b = s.GetNode(bId);
            if (a == null || b == null || aId == bId) return false;

            if (maxDegree > 0)
            {
                if (a.Links.Count >= maxDegree) return false;
                if (b.Links.Count >= maxDegree) return false;
            }

            a.AddLink(bId);
            b.AddLink(aId);
            return true;
        }

        // Standard BFS to mark reachability
        private void BfsFromStart(SectorMapState s, int startId, bool[] visited)
        {
            Array.Fill(visited, false);
            var q = new Queue<int>();
            visited[startId] = true;
            q.Enqueue(startId);

            while (q.Count > 0)
            {
                int u = q.Dequeue();
                var node = s.GetNode(u);
                for (int i = 0; i < node.Links.Count; i++)
                {
                    int v = node.Links[i];
                    if (!visited[v])
                    {
                        visited[v] = true;
                        q.Enqueue(v);
                    }
                }
            }
        }

        // Collect all nodes in a disconnected component starting at 'seed'
        private List<int> CollectComponent(SectorMapState s, int seed, bool[] seen)
        {
            var comp = new List<int>();
            var q = new Queue<int>();
            seen[seed] = true; q.Enqueue(seed);

            while (q.Count > 0)
            {
                int u = q.Dequeue();
                comp.Add(u);
                var node = s.GetNode(u);
                for (int i = 0; i < node.Links.Count; i++)
                {
                    int v = node.Links[i];
                    if (!seen[v]) { seen[v] = true; q.Enqueue(v); }
                }
            }
            return comp;
        }

        // Bridge every non-main component to the main one with a single adjacent-column link
        private void ConnectIslandsToMain(
            SectorMapState s, int cols, List<List<int>> buckets, SectorMapData data, System.Random rng)
        {
            int n = s.Nodes.Count;
            var visited = new bool[n];
            BfsFromStart(s, s.StartNodeId, visited);

            // While there are unvisited nodes, connect their component to the main set
            bool progress = true;
            while (progress)
            {
                progress = false;

                // Find an unvisited seed
                int seed = -1;
                for (int i = 0; i < n; i++)
                {
                    if (!visited[i])
                    {
                        seed = i;
                        break;
                    }
                }
                if (seed < 0) break;

                // Collect that component
                var seenLocal = new bool[n];
                var comp = CollectComponent(s, seed, seenLocal);

                // Pick the component node closest to main, but only consider same/adjacent columns
                int bestU = -1, bestV = -1;
                float bestDist = float.PositiveInfinity;

                foreach (var uId in comp)
                {
                    var u = s.GetNode(uId);
                    int cu = u.ColumnIndex;

                    // Candidates from main in columns [cu-1, cu, cu+1]
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int col = cu + dc;
                        if (col < 0 || col >= cols) continue;

                        var candidates = buckets[col];
                        for (int k = 0; k < candidates.Count; k++)
                        {
                            int vId = candidates[k];
                            if (!visited[vId]) continue; // must belong to main component

                            var v = s.GetNode(vId);

                            // Degree/ΔY checks before computing distance
                            if (data.maxDegreePerNode > 0 &&
                                (u.Links.Count >= data.maxDegreePerNode || v.Links.Count >= data.maxDegreePerNode))
                                continue;

                            float dy = Mathf.Abs(v.Position.y - u.Position.y);
                            if (data.maxCrossLinkDeltaY > 0f && dy > data.maxCrossLinkDeltaY) continue;

                            float dist = Vector2.SqrMagnitude(v.Position - u.Position);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestU = uId;
                                bestV = vId;
                            }
                        }
                    }
                }

                if (bestU >= 0 && bestV >= 0)
                {
                    if (TryLinkBothWays(s, bestU, bestV, data.maxDegreePerNode))
                    {
                        // Recompute visited after bridging
                        BfsFromStart(s, s.StartNodeId, visited);
                        progress = true;
                    }
                }
                else
                {
                    // As a fallback, connect seed to the start's column neighbor (shouldn't happen given spines)
                    int cu = s.GetNode(seed).ColumnIndex;
                    var candidates = buckets[Mathf.Clamp(cu, 0, cols - 1)];
                    if (candidates.Count > 0)
                    {
                        int vId = candidates[rng.Next(0, candidates.Count)];
                        TryLinkBothWays(s, seed, vId, data.maxDegreePerNode);
                        BfsFromStart(s, s.StartNodeId, visited);
                        progress = true;
                    }
                }
            }
        }
        #endregion

        #region Node Type Assignment
        // ---------------------------------------------------------------------
        // Node type assignment
        // ---------------------------------------------------------------------

        private void AssignNodeTypes(SectorMapState s, SectorMapData data, System.Random rng)
        {
            // Exit is Boss if requested
            if (data.forceBossOnExit)
                s.GetNode(s.ExitNodeId).Type = NodeType.Boss;

            // Build assignment pool (exclude Start/Exit)
            var pool = new List<SectorNodeState>();
            foreach (var n in s.Nodes)
            {
                if (n.Id == s.StartNodeId || n.Id == s.ExitNodeId) continue;
                pool.Add(n);
            }

            Shuffle(pool, rng);

            // Desired counts from rules
            var targets = new Dictionary<NodeType, int>();
            int poolCount = pool.Count;
            int assigned = 0;

            var rules = data.NodeCountRules; // public read-only accessor from SectorMapData
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule.type == NodeType.Boss) continue; // handled via exit
                int want = rng.Next(rule.min, rule.max + 1);
                targets[rule.type] = want;
                assigned += want;
            }

            // Fit targets to pool
            if (assigned > poolCount)
            {
                foreach (var k in new List<NodeType>(targets.Keys))
                {
                    if (assigned <= poolCount) break;
                    int delta = Math.Min(assigned - poolCount, Math.Max(0, targets[k] - 1));
                    targets[k] -= delta;
                    assigned -= delta;
                }
            }
            else if (assigned < poolCount)
            {
                int extra = poolCount - assigned;
                if (!targets.ContainsKey(NodeType.Gig)) targets[NodeType.Gig] = 0;
                targets[NodeType.Gig] += extra;
            }

            // Assign
            foreach (var kv in targets)
            {
                int count = kv.Value;
                for (int i = 0; i < count && pool.Count > 0; i++)
                {
                    var node = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                    node.Type = kv.Key;
                }
            }

            // Any leftovers default to Rehearsal
            foreach (var n in pool) n.Type = NodeType.Rehearsal;
        }

        #endregion

        private void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

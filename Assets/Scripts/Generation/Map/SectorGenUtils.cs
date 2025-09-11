using System;
using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Enums;

namespace ALWTTT.Generation
{
    /// <summary>Utility helpers reused by multiple stages.</summary>
    public static class SectorGenUtils
    {
        private static readonly List<int> s_empty = new();

        // ---------------- Layout ----------------

        public static List<List<int>> AllocateNodesPerColumn(int cols, int rows, int total, System.Random rng)
        {
            var result = new List<List<int>>(cols);
            for (int c = 0; c < cols; c++) result.Add(new List<int>());

            // guarantee one per column first
            int remaining = total;
            for (int c = 0; c < cols; c++)
            {
                int r = rng.Next(0, rows);
                result[c].Add(r);
            }
            remaining -= cols;

            // distribute remaining
            while (remaining > 0)
            {
                int c = rng.Next(0, cols);
                if (result[c].Count >= rows) continue;

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

            for (int c = 0; c < cols; c++) result[c].Sort();
            return result;
        }

        public static float RandomRange(System.Random rng, float min, float max)
            => (float)(min + (max - min) * rng.NextDouble());

        public static List<List<int>> BuildColumnBuckets(SectorMapState s, int cols)
        {
            var buckets = new List<List<int>>(cols);
            for (int c = 0; c < cols; c++) buckets.Add(new List<int>());
            foreach (var n in s.Nodes)
                if (n.ColumnIndex >= 0 && n.ColumnIndex < cols)
                    buckets[n.ColumnIndex].Add(n.Id);
            return buckets;
        }

        public static List<int> GetNodesInColumnFromBuckets(List<List<int>> buckets, int column)
            => (column >= 0 && column < buckets.Count) ? buckets[column] : s_empty;

        // ---------------- Selections ----------------

        public static int PickTopMost(IReadOnlyList<SectorNodeState> nodes, int column)
        {
            int best = -1; float bestY = float.PositiveInfinity;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.ColumnIndex != column) continue;
                if (n.Position.y < bestY) { bestY = n.Position.y; best = n.Id; }
            }
            return best;
        }

        public static int PickBottomMost(IReadOnlyList<SectorNodeState> nodes, int column)
        {
            int best = -1; float bestY = float.NegativeInfinity;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.ColumnIndex != column) continue;
                if (n.Position.y > bestY) { bestY = n.Position.y; best = n.Id; }
            }
            return best;
        }

        public static int PickNextWithVerticalBias(
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
                float score = Mathf.Pow(dy, Mathf.Max(1f, bias));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidates[i];
                }
            }
            return best;
        }

        // ---------------- Linking ----------------

        public static bool TryLinkBothWays(SectorMapState s, int aId, int bId, int maxDegree)
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

        public static void EnsureNoIsolatedForwardGaps(
            SectorMapState s, int cols, System.Random rng, List<List<int>> buckets, int maxDegree)
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
                        { hasAny = true; break; }
                    }
                    if (hasAny) break;
                }

                if (!hasAny && a.Count > 0 && b.Count > 0)
                {
                    int na = a[rng.Next(0, a.Count)];
                    int nb = b[rng.Next(0, b.Count)];
                    TryLinkBothWays(s, na, nb, maxDegree);
                }
            }
        }

        // ---------------- Connectivity (BFS) ----------------

        public static void BfsFromStart(SectorMapState s, int startId, bool[] visited)
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
                    if (!visited[v]) { visited[v] = true; q.Enqueue(v); }
                }
            }
        }

        public static List<int> CollectComponent(SectorMapState s, int seed, bool[] seen)
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

        public static void ConnectIslandsToMain(
            SectorMapState s, int cols, List<List<int>> buckets, SectorMapData data, System.Random rng)
        {
            int n = s.Nodes.Count;
            var visited = new bool[n];
            BfsFromStart(s, s.StartNodeId, visited);

            bool progress = true;
            while (progress)
            {
                progress = false;

                // find first unvisited id
                int seed = -1;
                for (int i = 0; i < n; i++)
                    if (!visited[i]) { seed = i; break; }

                if (seed < 0) break;

                // collect that component
                var seenLocal = new bool[n];
                var comp = CollectComponent(s, seed, seenLocal);

                int bestU = -1, bestV = -1;
                float bestDist = float.PositiveInfinity;

                foreach (var uId in comp)
                {
                    var u = s.GetNode(uId);
                    int cu = u.ColumnIndex;

                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int col = cu + dc;
                        if (col < 0 || col >= cols) continue;

                        var candidates = buckets[col];
                        for (int k = 0; k < candidates.Count; k++)
                        {
                            int vId = candidates[k];
                            if (!visited[vId]) continue;

                            var v = s.GetNode(vId);

                            if (data.maxDegreePerNode > 0 &&
                                (u.Links.Count >= data.maxDegreePerNode || v.Links.Count >= data.maxDegreePerNode))
                                continue;

                            float dy = Mathf.Abs(v.Position.y - u.Position.y);
                            if (data.maxCrossLinkDeltaY > 0f && dy > data.maxCrossLinkDeltaY) continue;

                            float dist = Vector2.SqrMagnitude(v.Position - u.Position);
                            if (dist < bestDist) { bestDist = dist; bestU = uId; bestV = vId; }
                        }
                    }
                }

                if (bestU >= 0 && bestV >= 0)
                {
                    if (TryLinkBothWays(s, bestU, bestV, data.maxDegreePerNode))
                    {
                        BfsFromStart(s, s.StartNodeId, visited);
                        progress = true;
                    }
                }
                else
                {
                    // Fallback: connect seed to any node in its column
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

        // ---------------- Types ----------------

        public static void AssignNodeTypes(SectorMapState s, SectorMapData data, System.Random rng)
        {
            if (data.forceBossOnExit)
                s.GetNode(s.ExitNodeId).Type = NodeType.Boss;

            var pool = new List<SectorNodeState>();
            foreach (var n in s.Nodes)
                if (n.Id != s.StartNodeId && n.Id != s.ExitNodeId) pool.Add(n);

            Shuffle(pool, rng);

            var targets = new Dictionary<NodeType, int>();
            int poolCount = pool.Count, assigned = 0;

            var rules = data.NodeCountRules;
            for (int i = 0; i < rules.Count; i++)
            {
                var r = rules[i];
                if (r.type == NodeType.Boss) continue;
                int want = rng.Next(r.min, r.max + 1);
                targets[r.type] = want;
                assigned += want;
            }

            if (assigned > poolCount)
            {
                foreach (var k in new List<NodeType>(targets.Keys))
                {
                    if (assigned <= poolCount) break;
                    int delta = Math.Min(assigned - poolCount, Math.Max(0, targets[k] - 1));
                    targets[k] -= delta; assigned -= delta;
                }
            }
            else if (assigned < poolCount)
            {
                int extra = poolCount - assigned;
                if (!targets.ContainsKey(NodeType.Gig)) targets[NodeType.Gig] = 0;
                targets[NodeType.Gig] += extra;
            }

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

            foreach (var n in pool) n.Type = NodeType.Rehearsal;
        }

        public static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Enums;

namespace ALWTTT.Generation
{
    public class LayoutAndCreateNodesStage : ISectorGenStage
    {
        public string Name => "Layout & Create Nodes";

        public IEnumerator Execute(SectorGraphContext ctx)
        {
            var data = ctx.Data; var rng = ctx.Rng;
            int cols = Mathf.Max(2, data.columns);
            int rows = Mathf.Max(2, data.rows);

            int totalNodes = Mathf.Clamp(
                rng.Next(data.totalNodesRange.x, data.totalNodesRange.y + 1),
                cols, cols * rows);

            var rowsPerColumn = 
                SectorGenUtils.AllocateNodesPerColumn(cols, rows, totalNodes, rng);

            int nextId = 0;
            for (int c = 0; c < cols; c++)
            {
                foreach (int r in rowsPerColumn[c])
                {
                    ctx.State.Nodes.Add(new SectorNodeState
                    {
                        Id = nextId++,
                        ColumnIndex = c,
                        Position = new Vector2(
                            c + 
                            SectorGenUtils.RandomRange(
                                rng, -data.positionJitter.x, data.positionJitter.x),
                            r + 
                            SectorGenUtils.RandomRange(
                                rng, -data.positionJitter.y, data.positionJitter.y)),
                        Type = NodeType.Rehearsal,
                        Links = new List<int>(),
                        Visited = false,
                        Locked = false
                    });
                }
            }

            ctx.Buckets = SectorGenUtils.BuildColumnBuckets(ctx.State, cols);
            yield break;
        }
    }
}

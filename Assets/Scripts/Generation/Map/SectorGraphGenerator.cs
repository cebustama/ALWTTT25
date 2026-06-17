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
            var stepper = CreateStepper(sectorId, data, rng);
            stepper.RunToEnd();
            return stepper.Context.State;
        }

        public SectorGraphStepper CreateStepper(int sectorId, SectorMapData data, System.Random rng = null)
        {
            rng ??= (data.fixedSeed >= 0)
                ? new System.Random(data.fixedSeed)
                : new System.Random(Guid.NewGuid().GetHashCode());

            var ctx = new SectorGraphContext(sectorId, data, rng);

            var stages = new List<ISectorGenStage>
            {
                new LayoutAndCreateNodesStage(),
                new PickStartExitStage(),
                new BuildSpinesStage(),
                new CrossLinksStage(),
                new RepairGapsStage(),
                new ConnectivityStage(),
                new AssignTypesStage()
            };

            return new SectorGraphStepper(stages, ctx);
        }
    }
}

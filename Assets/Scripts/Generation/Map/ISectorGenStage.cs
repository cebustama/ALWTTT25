using ALWTTT.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Generation
{
    /// <summary> Small, focused unit of work that mutates the graph context. </summary>
    public interface ISectorGenStage
    {
        string Name { get; }
        /// <summary>
        /// Execute the stage. Return as IEnumerator so we can yield sub-steps if needed.
        /// For simple stages, yield once at the end (or not at all and just return null).
        /// </summary>
        IEnumerator Execute(SectorGraphContext ctx);
    }

    /// <summary> DTO visible to UI for debug/teaching purposes. </summary>
    public struct GenStepInfo
    {
        public string StageName;
        public string Detail;
        public int StepIndex;  // which step of the pipeline we’re on
    }

    /// <summary>
    /// Holds all mutable state and utilities that stages need.
    /// </summary>
    public class SectorGraphContext
    {
        public int SectorId;
        public SectorMapData Data;
        public System.Random Rng;

        public SectorMapState State;
        public List<List<int>> Buckets;

        public SectorGraphContext(int sectorId, SectorMapData data, System.Random rng)
        {
            SectorId = sectorId;
            Data = data;
            Rng = rng;
            State = new SectorMapState 
            { 
                SectorId = sectorId, 
                Nodes = new List<SectorNodeState>() 
            };
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Serializable runtime state for a whole sector map.
    /// This is saved/restored inside PersistentGameplayData.
    /// </summary>
    [Serializable]
    public class SectorMapState
    {
        [SerializeField] private int sectorId;
        [SerializeField] private List<SectorNodeState> nodes = new();
        [SerializeField] private int startNodeId;
        [SerializeField] private int exitNodeId;
        [SerializeField] private int currentNodeId;
        [SerializeField] private bool sectorCleared;

        #region Encapsulation
        public int SectorId { get => sectorId; set => sectorId = value; }
        public List<SectorNodeState> Nodes { get => nodes; set => nodes = value; }
        public int StartNodeId { get => startNodeId; set => startNodeId = value; }
        public int ExitNodeId { get => exitNodeId; set => exitNodeId = value; }
        public int CurrentNodeId { get => currentNodeId; set => currentNodeId = value; }
        public bool SectorCleared { get => sectorCleared; set => sectorCleared = value; }
        #endregion

        #region Helpers
        public SectorNodeState GetNode(int id) => nodes.Find(n => n.Id == id);

        public bool TryGetNode(int id, out SectorNodeState node)
        {
            node = nodes.Find(n => n.Id == id);
            return node != null;
        }

        public IReadOnlyList<int> GetNeighbors(int id)
        {
            var n = GetNode(id);
            return n != null ? n.Links : Array.Empty<int>();
        }
        #endregion
    }
}
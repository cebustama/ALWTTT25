using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Serializable runtime state for a single node in the sector map.
    /// </summary>
    [Serializable]
    public class SectorNodeState
    {
        [SerializeField] private int id;
        [SerializeField] private int columnIndex;
        [SerializeField] private Vector2 position;  // UI-space position (logical grid + jitter)
        [SerializeField] private NodeType type;
        [SerializeField] private bool visited;
        [SerializeField] private bool locked;
        [SerializeField] private bool completed; // true once resolved (for gigs: after win OR loss)
        [SerializeField] private List<int> links = new(); // Adjacent node ids

        [SerializeField] private int gigEncounterIndex = -1;    // -1 = use random
        [SerializeField] private List<string> recruitIds = new();   // candidates shown here
        [SerializeField] private string randomEventId;  // id to replay the same event

        #region Encapsulation
        public int Id { get => id; set => id = value; }
        public int ColumnIndex { get => columnIndex; set => columnIndex = value; }
        public Vector2 Position { get => position; set => position = value; }
        public NodeType Type { get => type; set => type = value; }
        public bool Visited { get => visited; set => visited = value; }
        public bool Locked { get => locked; set => locked = value; }
        public bool Completed { get => completed; set => completed = value; }
        public List<int> Links { get => links; set => links = value; }

        public int GigEncounterIndex { get => gigEncounterIndex; set => gigEncounterIndex = value; }
        public List<string> RecruitIds { get => recruitIds; set => recruitIds = value; }
        public string RandomEventId { get => randomEventId; set => randomEventId = value; }
        #endregion

        #region Convenience
        public bool HasFixedGig => gigEncounterIndex >= 0;
        public void AddLink(int otherId)
        {
            if (!links.Contains(otherId)) links.Add(otherId);
        }
        public void RemoveLink(int otherId)
        {
            if (links.Contains(otherId)) links.Remove(otherId);
        }
        #endregion
    }
}
using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Enums;

namespace ALWTTT.Map
{
    /// <summary>
    /// Presentation-only controller for the Sector Map.
    /// Spawns node/link visuals from a SectorMapState and keeps them in sync.
    /// No game logic or state decisions here.
    /// </summary>
    public class SectorMapVisual : MonoBehaviour
    {
        [Header("Prefabs & Roots")]
        [SerializeField] private SectorNodeVisual nodePrefab;
        [SerializeField] private SectorLinkVisual linkPrefab;
        [SerializeField] private Transform nodesRoot;
        [SerializeField] private Transform linksRoot;

        [Header("Grid Mapping")]
        [Tooltip("Logical grid (x,y) -> world position")]
        [SerializeField] private Vector2 gridSpacing = new(2.0f, 1.4f);
        [SerializeField] private Vector2 gridOrigin = new(-7.5f, -3.5f);
        [SerializeField] private float nodeZ = 0f;
        [SerializeField] private float linkZ = 0f;

        [Header("Colors")]
        [SerializeField] private Color rehearsalColor = new(0.2f, 0.8f, 1f);
        [SerializeField] private Color gigColor = new(1f, 0.6f, 0.2f);
        [SerializeField] private Color randomColor = new(0.9f, 0.9f, 0.2f);
        [SerializeField] private Color recruitColor = new(0.4f, 1f, 0.4f);
        [SerializeField] private Color bossColor = new(1f, 0.2f, 0.4f);

        private SectorMapState _state;
        private readonly Dictionary<int, SectorNodeVisual> _nodeViews = new();
        private readonly List<SectorLinkVisual> _linkViews = new();

        #region Public API
        public void Render(SectorMapState state)
        {
            _state = state;
            Clear();

            if (_state == null || nodePrefab == null || linkPrefab == null || nodesRoot == null || linksRoot == null)
            {
                Debug.LogWarning("SectorMapVisual: Missing references. Assign prefabs/roots.");
                return;
            }

            // 1) Nodes
            foreach (var node in _state.Nodes)
            {
                var pos = GridToWorld(node.Position);
                var view = Instantiate(nodePrefab, pos, Quaternion.identity, nodesRoot);
                view.Bind(node, GetColor(node.Type), 0.5f);
                view.SetVisited(node.Visited);
                view.SetSelected(node.Id == _state.CurrentNodeId);
                _nodeViews[node.Id] = view;
            }

            // 2) Links (unique: only a<b)
            foreach (var node in _state.Nodes)
            {
                for (int i = 0; i < node.Links.Count; i++)
                {
                    int otherId = node.Links[i];
                    if (node.Id >= otherId) continue;

                    var a = _nodeViews[node.Id].transform;
                    var b = _nodeViews[otherId].transform;

                    var link = Instantiate(linkPrefab, linksRoot);
                    link.Bind(a, b, linkZ);
                    _linkViews.Add(link);
                }
            }
        }

        public void SyncNodeStates()
        {
            if (_state == null) return;

            foreach (var kv in _nodeViews)
            {
                var view = kv.Value;
                var node = view.Node;
                view.SetVisited(node.Visited);
                view.SetSelected(node.Id == _state.CurrentNodeId);
            }
        }

        public void Clear()
        {
            _nodeViews.Clear();
            _linkViews.Clear();

            if (nodesRoot)
                for (int i = nodesRoot.childCount - 1; i >= 0; i--)
                    Destroy(nodesRoot.GetChild(i).gameObject);

            if (linksRoot)
                for (int i = linksRoot.childCount - 1; i >= 0; i--)
                    Destroy(linksRoot.GetChild(i).gameObject);
        }
        #endregion

        private void LateUpdate()
        {
            // Keep link visuals in sync each frame (cheap).
            for (int i = 0; i < _linkViews.Count; i++)
                _linkViews[i].UpdatePositions(linkZ);
        }

        private Vector3 GridToWorld(Vector2 gridPos)
        {
            return new Vector3(
                gridOrigin.x + gridPos.x * gridSpacing.x,
                gridOrigin.y + gridPos.y * gridSpacing.y,
                nodeZ
            );
        }

        private Color GetColor(NodeType t) => t switch
        {
            NodeType.Rehearsal => rehearsalColor,
            NodeType.Gig => gigColor,
            NodeType.RandomEncounter => randomColor,
            NodeType.Recruit => recruitColor,
            NodeType.Boss => bossColor,
            _ => Color.white
        };
    }
}

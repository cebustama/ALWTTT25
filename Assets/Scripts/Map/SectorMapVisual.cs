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

        [Header("Data (for tooltips)")]
        // TODO: Receive from SectorMapManager
        [SerializeField] private SectorMapData sectorMapData;

        private SectorMapState _state;
        private readonly Dictionary<int, SectorNodeVisual> _nodeViews = new();
        private readonly List<SectorLinkVisual> _linkViews = new();
        private readonly Dictionary<int, List<SectorLinkVisual>> _linksByNodeId = new();
        private int? _previewNodeId = null;

        public System.Action<SectorNodeState> NodeClicked;

        #region Public API

        public void Render(SectorMapState state)
        {
            _state = state;
            Clear();

            if (_state == null || nodePrefab == null 
                || linkPrefab == null || nodesRoot == null || linksRoot == null)
            {
                Debug.LogWarning("SectorMapVisual: Missing references. Assign prefabs/roots.");
                return;
            }

            // nodes
            foreach (var node in _state.Nodes)
            {
                var pos = GridToWorld(node.Position);
                var view = Instantiate(nodePrefab, pos, Quaternion.identity, nodesRoot);

                // Tooltip text
                var title = sectorMapData ? 
                        sectorMapData.GetNodeTypeTitle(node.Type) : 
                        node.Type.ToString();

                var desc = sectorMapData ? 
                    sectorMapData.GetNodeTypeDescription(node.Type) : "";

                view.Bind(node, GetColor(node.Type), title, desc, 0.5f);
                view.SetVisited(node.Visited);
                view.SetSelected(node.Id == _state.CurrentNodeId);

                view.Clicked += OnNodeClicked;
                view.HoverEnter += OnNodeHoverEnter;
                view.HoverExit += OnNodeHoverExit;

                _nodeViews[node.Id] = view;
                _linksByNodeId[node.Id] = new List<SectorLinkVisual>();
            }

            // links
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

                    _linksByNodeId[node.Id].Add(link);
                    _linksByNodeId[otherId].Add(link);
                }
            }

            // Default view: only current node links visible
            _previewNodeId = null;
            ApplyLinkVisibility();
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

            // Keep link visibility consistent with the "current" selection
            ApplyLinkVisibility();
        }

        public void Clear()
        {
            foreach (var v in _nodeViews.Values)
            {
                v.Clicked -= OnNodeClicked;
                v.HoverEnter -= OnNodeHoverEnter;
                v.HoverExit -= OnNodeHoverExit;
            }

            _nodeViews.Clear();
            _linkViews.Clear();
            _linksByNodeId.Clear();

            if (nodesRoot)
                for (int i = nodesRoot.childCount - 1; i >= 0; i--)
                    Destroy(nodesRoot.GetChild(i).gameObject);

            if (linksRoot)
                for (int i = linksRoot.childCount - 1; i >= 0; i--)
                    Destroy(linksRoot.GetChild(i).gameObject);
        }

        public Transform GetNodeTransform(int nodeId)
        {
            return _nodeViews.TryGetValue(nodeId, out var view) ? view.transform : null;
        }
        #endregion

        private void LateUpdate()
        {
            // Keep link visuals in sync each frame (cheap).
            for (int i = 0; i < _linkViews.Count; i++)
                _linkViews[i].UpdatePositions(linkZ);
        }

        private void OnNodeClicked(SectorNodeVisual visual)
        {
            NodeClicked?.Invoke(visual.Node);
        }

        private void OnNodeHoverEnter(SectorNodeVisual visual)
        {
            _previewNodeId = visual.Node.Id; // set preview
            ApplyLinkVisibility();
        }

        private void OnNodeHoverExit(SectorNodeVisual visual)
        {
            _previewNodeId = null; // clear preview
            ApplyLinkVisibility();
        }

        public void ShowLinksForCurrentOnly()
        {
            _previewNodeId = null;
            ApplyLinkVisibility();
        }

        // --------- Link visibility control ---------

        private void SetAllLinksVisible(bool visible, bool emphasize = false)
        {
            for (int i = 0; i < _linkViews.Count; i++)
            {
                _linkViews[i].SetVisible(visible);
                _linkViews[i].SetEmphasis(emphasize && visible);
            }
        }

        private void ApplyLinkVisibility()
        {
            // 1) hide all
            for (int i = 0; i < _linkViews.Count; i++)
            {
                _linkViews[i].SetVisible(false);
                _linkViews[i].SetEmphasis(false); // reset to base width when (re)shown
            }

            // 2) show preview (base width)
            if (_previewNodeId.HasValue 
                && _linksByNodeId.TryGetValue(_previewNodeId.Value, out var preview))
            {
                for (int i = 0; i < preview.Count; i++)
                {
                    preview[i].SetVisible(true);
                    preview[i].SetEmphasis(false); // base width for hover preview
                }
            }

            // 3) show current (emphasized) — always visible, even while hovering others
            if (_state != null
                && _linksByNodeId.TryGetValue(_state.CurrentNodeId, out var current))
            {
                for (int i = 0; i < current.Count; i++)
                {
                    current[i].SetVisible(true);
                    current[i].SetEmphasis(true); // emphasized width for current
                }
            }
        }

        // ------ Helpers ------

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

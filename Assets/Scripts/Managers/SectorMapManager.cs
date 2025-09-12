using UnityEngine;
using TMPro;
using ALWTTT.Data;
using ALWTTT.Generation;
using ALWTTT.Map;
using System.Collections;
using ALWTTT.Utils;

namespace ALWTTT.Managers
{
    /// <summary>
    /// Scene logic holder. Owns the generation flow (immediate or stepped),
    /// writes the resulting SectorMapState into persistent data,
    /// and delegates rendering to SectorMapVisual.
    /// </summary>
    public class SectorMapManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SectorMapData sectorMapData;

        [Header("Visual Controller")]
        [SerializeField] private SectorMapVisual mapVisual;

        [Header("Ship")]
        [SerializeField] private ShipController shipPrefab;
        private ShipController _ship;

        [Header("HUD (stats)")]
        [SerializeField] private TextMeshProUGUI fansText;
        [SerializeField] private TextMeshProUGUI cohesionText;

        [Header("References")]
        [SerializeField] private SceneChanger sceneChanger;

        [Header("HUD (debug / step-by-step)")]
        [SerializeField] private bool stepMode = false; // toggle in Inspector
        [SerializeField] private TextMeshProUGUI nodeCountText; // "# Nodes = N"
        [SerializeField] private TextMeshProUGUI linkCountText; // "# Links = M"
        [SerializeField] private TextMeshProUGUI stageNameText; // "Stage: Name"

        private SectorGraphStepper _stepper;    // when stepMode is ON
        private GameManager GM => GameManager.Instance;

        private SectorMapState State
        {
            get => GM.PersistentGameplayData.CurrentSectorMapState;
            set => GM.PersistentGameplayData.CurrentSectorMapState = value;
        }

        private bool _isResolving;

        private void Start()
        {
            if (!NodeResolverProcessor.IsInitialized)
                NodeResolverProcessor.Initialize();

            if (stepMode)
            {
                // In step mode we start with no state until the user presses R.
                ClearOverlay("(press R to start)");
                mapVisual.Clear();
                RefreshHUD();
            }
            else
            {
                // Normal “instant” generation
                // TODO: MapDone method
                EnsureStateImmediate();
                AttachEncountersToNodes();
                SafeRenderAndReattach();
                mapVisual.NodeClicked += HandleNodeClicked;
                RefreshHUD();
                UpdateDebugOverlay("ready");
            }
        }

        private void Update()
        {
            // Start generation (create stepper) or regenerate instantly
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (stepMode)
                {
                    CreateStepper();
                    // Show initial (empty links) state right away
                    SafeRenderAndReattach();
                    UpdateDebugOverlay("ready");
                }
                else
                {
                    RegenerateImmediate();
                }
            }

            // Advance one generation stage
            if (stepMode && _stepper != null && Input.GetKeyDown(KeyCode.Space))
            {
                if (!_stepper.IsDone)
                {
                    var info = _stepper.Step();
                    // State object inside the stepper is the same reference we stored
                    SafeRenderAndReattach();
                    UpdateDebugOverlay(info.StageName);
                }
                else
                {
                    // TODO: MapDone method
                    mapVisual.NodeClicked += HandleNodeClicked;
                    AttachEncountersToNodes();
                    UpdateDebugOverlay("(done)");
                }
            }

            // Optional shortcut: run all remaining stages with Enter
            if (stepMode && _stepper != null && Input.GetKeyDown(KeyCode.Return))
            {
                if (!_stepper.IsDone)
                {
                    _stepper.RunToEnd();
                    // TODO: MapDone method
                    AttachEncountersToNodes();
                    SafeRenderAndReattach();
                    mapVisual.NodeClicked += HandleNodeClicked;
                }
                UpdateDebugOverlay("(done)");
            }

            // If something in state changes at runtime (e.g., moving current node),
            // keep visuals (visited/selected) in sync:
            mapVisual.SyncNodeStates();
        }

        private void AttachEncountersToNodes()
        {
            var pd = GameManager.Instance.PersistentGameplayData;
            var ed = GameManager.Instance.EncounterData;

            // Ensure sector id is set (first entry to Map)
            if (pd.CurrentSectorId < 0) pd.CurrentSectorId = 0;

            var sector = ed.EncounterSectorsList.Find(s => s.SectorId == pd.CurrentSectorId);
            if (sector == null) return;

            // NORMAL gigs
            var gigNodes = State.Nodes.FindAll(n => n.Type == Enums.NodeType.Gig);
            var gigCount = sector.GigEncounterList?.Count ?? 0;
            if (gigCount > 0)
            {
                // Simple distribution: assign indices round-robin (or shuffle first if desired)
                for (int i = 0; i < gigNodes.Count; i++)
                    gigNodes[i].GigEncounterIndex = i % gigCount;
            }

            // BOSS gigs (often 1, but support many)
            var bossNodes = State.Nodes.FindAll(n => n.Type == Enums.NodeType.Boss);
            var bossCount = sector.BossGigEncounterList?.Count ?? 0;
            if (bossCount > 0)
            {
                for (int i = 0; i < bossNodes.Count; i++)
                    bossNodes[i].GigEncounterIndex = i % bossCount;
            }
        }
        private void SafeRenderAndReattach()
        {
            if (_ship) _ship.transform.SetParent(null, worldPositionStays: true);

            mapVisual.Render(State);

            EnsureShipAttachedToCurrentNode();

            // Ensure link visibility reflects "current" after a render
            mapVisual.ShowLinksForCurrentOnly();
        }

        private void EnsureShipAttachedToCurrentNode()
        {
            if (State == null) return;
            var nodeT = mapVisual.GetNodeTransform(State.CurrentNodeId);
            if (nodeT == null) return; // in early step stages nodes may not exist yet

            if (_ship == null)
            {
                _ship = Instantiate(shipPrefab);
            }

            _ship.AttachTo(nodeT);
        }

        private void MoveShipToNode(int nodeId)
        {
            var nodeT = mapVisual.GetNodeTransform(nodeId);
            if (nodeT && _ship) _ship.AttachTo(nodeT);
        }


        private void HandleNodeClicked(SectorNodeState node)
        {
            if (_isResolving) return;
            // Only allow moves to neighbors
            if (!IsNeighbor(State.CurrentNodeId, node.Id))
                return;

            // Prevent revisiting completed Gig nodes
            if ((node.Type == Enums.NodeType.Gig 
                || node.Type == Enums.NodeType.Boss) && node.Completed)
                return;

            // Update state and visuals
            State.CurrentNodeId = node.Id;
            node.Visited = true;
            mapVisual.SyncNodeStates();
            mapVisual.ShowLinksForCurrentOnly();
            MoveShipToNode(node.Id);

            // ---- Go to gig if needed ----
            if (node.Type == Enums.NodeType.Gig || node.Type == Enums.NodeType.Boss)
            {
                var pd = GameManager.Instance.PersistentGameplayData;
                pd.CurrentEncounterId = Mathf.Max(0, node.GigEncounterIndex);
                pd.IsFinalEncounter = node.Type == Enums.NodeType.Boss;
                pd.LastMapNodeId = node.Id;

                sceneChanger.OpenGigScene();
                return;
            }

            // Resolve the node interaction
            StartCoroutine(ResolveCurrentNode());
        }

        private IEnumerator ResolveCurrentNode()
        {
            _isResolving = true;

            var node = State.GetNode(State.CurrentNodeId);
            var resolver = NodeResolverProcessor.Get(node.Type);

            var ctx = new NodeResolveContext(
                manager: this,
                mapData: sectorMapData,
                persistent: GameManager.Instance.PersistentGameplayData,
                mapState: State,
                visual: mapVisual);

            yield return resolver.Resolve(ctx, node);

            _isResolving = false;

            // After resolve, auto-update links/visited visuals again
            mapVisual.SyncNodeStates();
            mapVisual.ShowLinksForCurrentOnly();
        }

        private bool IsNeighbor(int fromId, int toId)
        {
            var n = State.GetNode(fromId);
            if (n == null) return false;
            for (int i = 0; i < n.Links.Count; i++)
                if (n.Links[i] == toId) return true;
            return false;
        }

        private void OnDestroy()
        {
            if (mapVisual) mapVisual.NodeClicked -= HandleNodeClicked;
        }

        // ---------------- Immediate path (non-step mode) ----------------

        private void EnsureStateImmediate()
        {
            int sectorId = GM.PersistentGameplayData.CurrentSectorId;
            if (State == null || State.SectorId != sectorId)
            {
                var gen = new SectorGraphGenerator();
                State = gen.Generate(sectorId, sectorMapData, null);
            }
        }

        [ContextMenu("Regenerate (Immediate)")]
        public void RegenerateImmediate()
        {
            var gen = new SectorGraphGenerator();
            State = gen.Generate(GM.PersistentGameplayData.CurrentSectorId, sectorMapData, null);
            SafeRenderAndReattach();
            RefreshHUD();
            UpdateDebugOverlay("done");
        }

        // ---------------- Step-by-step path ----------------

        private void CreateStepper()
        {
            var gen = new SectorGraphGenerator();
            _stepper = gen.CreateStepper(GM.PersistentGameplayData.CurrentSectorId, sectorMapData, null);

            // IMPORTANT: Persist the new (empty) State reference so it’s visible across the app
            State = _stepper.Context.State;
            SafeRenderAndReattach();
            RefreshHUD();
        }

        // ---------------- HUD / overlay ----------------

        public void RefreshHUD()
        {
            var pd = GM.PersistentGameplayData;
            if (fansText) fansText.text = $"Fans: {pd.Fans}";
            if (cohesionText) cohesionText.text = $"Cohesion: {pd.BandCohesion}";
        }

        private void UpdateDebugOverlay(string stageName)
        {
            if (State != null)
            {
                if (nodeCountText) nodeCountText.text = $"# Nodes = {State.Nodes.Count}";
                if (linkCountText) linkCountText.text = $"# Links = {CountUniqueLinks(State)}";
            }
            else
            {
                ClearOverlay(stageName);
                return;
            }

            if (stageNameText)
            {
                // If we are in stepMode and have a stepper, show the next/current stage nicely
                if (stepMode && _stepper != null && !_stepper.IsDone && stageName == "ready")
                    stageNameText.text = $"Stage: (next) {_stepper.CurrentStageName}";
                else
                    stageNameText.text = $"Stage: {stageName}";
            }
        }

        private void ClearOverlay(string stageName)
        {
            if (nodeCountText) nodeCountText.text = "# Nodes = 0";
            if (linkCountText) linkCountText.text = "# Links = 0";
            if (stageNameText) stageNameText.text = stageName;
        }

        private static int CountUniqueLinks(SectorMapState state)
        {
            if (state == null || state.Nodes == null) return 0;
            int count = 0;
            for (int i = 0; i < state.Nodes.Count; i++)
            {
                var n = state.Nodes[i];
                for (int j = 0; j < n.Links.Count; j++)
                {
                    if (n.Id < n.Links[j]) count++; // count each undirected edge once
                }
            }
            return count;
        }
    }
}

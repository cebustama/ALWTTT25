using UnityEngine;
using TMPro;
using ALWTTT.Data;
using ALWTTT.Generation;
using ALWTTT.Map;

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

        [Header("HUD (stats)")]
        [SerializeField] private TextMeshProUGUI fansText;
        [SerializeField] private TextMeshProUGUI cohesionText;

        [Header("HUD (debug / step-by-step)")]
        [SerializeField] private bool stepMode = false;                     // toggle in Inspector
        [SerializeField] private TextMeshProUGUI nodeCountText;             // "# Nodes = N"
        [SerializeField] private TextMeshProUGUI linkCountText;             // "# Links = M"
        [SerializeField] private TextMeshProUGUI stageNameText;             // "Stage: Name"

        private SectorGraphStepper _stepper;                                // when stepMode is ON
        private GameManager GM => GameManager.Instance;

        private SectorMapState State
        {
            get => GM.PersistentGameplayData.CurrentSectorMapState;
            set => GM.PersistentGameplayData.CurrentSectorMapState = value;
        }

        private void Start()
        {
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
                EnsureStateImmediate();
                mapVisual.Render(State);
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
                    mapVisual.Render(State);
                    mapVisual.NodeClicked += HandleNodeClicked;
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
                    mapVisual.Render(State);
                    UpdateDebugOverlay(info.StageName);
                }
                else
                {
                    UpdateDebugOverlay("(done)");
                }
            }

            // Optional shortcut: run all remaining stages with Enter
            if (stepMode && _stepper != null && Input.GetKeyDown(KeyCode.Return))
            {
                if (!_stepper.IsDone)
                {
                    _stepper.RunToEnd();
                    mapVisual.Render(State);
                }
                UpdateDebugOverlay("(done)");
            }

            // If something in state changes at runtime (e.g., moving current node),
            // keep visuals (visited/selected) in sync:
            mapVisual.SyncNodeStates();
        }

        private void HandleNodeClicked(SectorNodeState node)
        {
            // Demo: select the clicked node and mark as visited
            State.CurrentNodeId = node.Id;
            node.Visited = true;
            mapVisual.SyncNodeStates();

            // Later: add rules (only allow traveling to neighbors, trigger scenes, etc.)
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
            mapVisual.Render(State);
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

            RefreshHUD();
        }

        // ---------------- HUD / overlay ----------------

        private void RefreshHUD()
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

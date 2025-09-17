using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Events;
using ALWTTT.Generation;
using ALWTTT.Map;
using ALWTTT.UI;
using ALWTTT.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        // TODO Move to UIManager
        [SerializeField] private TextMeshProUGUI fansText;
        [SerializeField] private TextMeshProUGUI cohesionText;
        [SerializeField] private MusicianMapStatusUI musicianStatusPrefab;
        [SerializeField] private Transform musicianStatusRoot;

        private Dictionary<string, MusicianMapStatusUI> musicianStatusDict;

        [Header("Events Canvas")]
        [SerializeField] private RandomEventCanvas randomEventCanvas;

        [Header("References")]
        [SerializeField] private SceneChanger sceneChanger;
        [SerializeField] private RecruitCanvas recruitCanvas; // TODO move to UIManager
        [SerializeField] private Transform modalParent;

        [Header("HUD (debug / step-by-step)")]
        [SerializeField] private bool stepMode = false; // toggle in Inspector
        [SerializeField] private TextMeshProUGUI nodeCountText; // "# Nodes = N"
        [SerializeField] private TextMeshProUGUI linkCountText; // "# Links = M"
        [SerializeField] private TextMeshProUGUI stageNameText; // "Stage: Name"

        private SectorGraphStepper _stepper;    // when stepMode is ON
        private GameManager GameManager => GameManager.Instance;

        private SectorMapState State
        {
            get => GameManager.PersistentGameplayData.CurrentSectorMapState;
            set => GameManager.PersistentGameplayData.CurrentSectorMapState = value;
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

            var pd = GameManager.Instance.PersistentGameplayData;

            // Update state and visuals
            State.CurrentNodeId = node.Id;
            node.Visited = true;
            mapVisual.SyncNodeStates();
            mapVisual.ShowLinksForCurrentOnly();
            MoveShipToNode(node.Id);

            // ---- Go to gig if needed ----
            if (node.Type == Enums.NodeType.Gig || node.Type == Enums.NodeType.Boss)
            {
                pd.CurrentEncounterId = Mathf.Max(0, node.GigEncounterIndex);
                pd.IsFinalEncounter = node.Type == Enums.NodeType.Boss;
                pd.LastMapNodeId = node.Id;

                sceneChanger.OpenGigScene();
                return;
            }

            if (node.Type == Enums.NodeType.Rehearsal)
            {
                pd.LastMapNodeId = node.Id;

                sceneChanger.OpenShipScene();
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
            int sectorId = GameManager.PersistentGameplayData.CurrentSectorId;
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
            State = gen.Generate(GameManager.PersistentGameplayData.CurrentSectorId, sectorMapData, null);
            SafeRenderAndReattach();
            RefreshHUD();
            UpdateDebugOverlay("done");
        }

        // ---------------- Step-by-step path ----------------

        private void CreateStepper()
        {
            var gen = new SectorGraphGenerator();
            _stepper = gen.CreateStepper(GameManager.PersistentGameplayData.CurrentSectorId, sectorMapData, null);

            // IMPORTANT: Persist the new (empty) State reference so it’s visible across the app
            State = _stepper.Context.State;
            SafeRenderAndReattach();
            RefreshHUD();
        }

        // ---------------- HUD / overlay ----------------

        public void RefreshHUD()
        {
            var pd = GameManager.PersistentGameplayData;
            if (fansText) fansText.text = $"Fans: {pd.Fans}";
            if (cohesionText) cohesionText.text = $"Cohesion: {pd.BandCohesion}";

            RefreshMusicianStatusInfo();
        }

        private void RefreshMusicianStatusInfo()
        {
            if (musicianStatusDict == null)
                musicianStatusDict = new Dictionary<string, MusicianMapStatusUI>();

            var pd = GameManager.PersistentGameplayData;

            foreach (var mus in pd.MusicianList)
            {
                var id = mus.MusicianCharacterData.CharacterId;
                var health = pd.GetMusicianHealthData(id);

                if (health == null) 
                    health = pd.SetMusicianHealthData(
                        id, 0, mus.MusicianCharacterData.InitialMaxStress);

                if (musicianStatusDict.ContainsKey(id))
                {
                    var status = musicianStatusDict[id];
                    status.SetStress(health.CurrentStress, health.MaxStress);
                }
                else
                {
                    var status = Instantiate(musicianStatusPrefab, musicianStatusRoot);
                    status.SetName(mus.MusicianCharacterData.CharacterName);
                    status.SetStress(health.CurrentStress, health.MaxStress);
                    musicianStatusDict.Add(id, status);
                }
            }
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

        private T EnsureModal<T>(T prefab) where T : Component
        {
            // Try to find an existing instance in scene first
            var existing = FindObjectOfType<T>(includeInactive: true);
            if (existing) return existing;

            // Otherwise instantiate
            return Instantiate(prefab, modalParent ? modalParent : transform);
        }

        private IEnumerator OpenModalAndWait<TCanvas, TResult>(
            TCanvas canvas,
            Action<TCanvas, Action<TResult>> showWithComplete,
            Action<TResult> onDone)
            where TCanvas : Component
        {
            bool finished = false;
            TResult result = default;

            showWithComplete?.Invoke(
                canvas, 
                r => 
                { 
                    result = r; 
                    finished = true; 
                }
            );

            yield return new WaitUntil(() => finished);

            onDone?.Invoke(result);
        }

        public IEnumerator ShowRecruit(Action<MusicianCharacterData> onChosen)
        {
            List<MusicianBase> candidates = 
                GameManager.PersistentGameplayData.AvailableMusiciansList;

            RecruitCanvas canvas = EnsureModal(recruitCanvas);

            yield return OpenModalAndWait<RecruitCanvas, MusicianCharacterData>(
                canvas,
                (c, complete) => c.Show(candidates, complete),
                chosen => onChosen?.Invoke(chosen));
        }

        public IEnumerator ShowRandomEvent(Action<RandomEventOption> onChosen)
        {
            var data = PickEventForSector(GameManager.PersistentGameplayData.CurrentSectorId);
            Debug.Log($"<color=white>{data} picked...</color>");

            var canvas = EnsureModal(randomEventCanvas);
            yield return OpenModalAndWait<RandomEventCanvas, RandomEventOption>(
                canvas,
                (c, complete) => c.Show(data, complete),
                chosen => onChosen?.Invoke(chosen));
        }

        RandomEventData PickEventForSector(int sectorId)
        {
            var gameplayData = GameManager.GameplayData;
            var persistent = GameManager.PersistentGameplayData;
            var table = gameplayData.EventTables.Find(t => t.tableId == $"Sector_{sectorId}");
            if (table == null) table = gameplayData.EventTables.Find(t => t.tableId == "Common");
            if (table == null) return null;

            var eligible = new List<(RandomEventData data, int weight)>();

            // Find eligible events
            foreach (var e in table.entries)
            {
                if (e.data == null) continue;

                // sector gate
                if (sectorId < e.minSector || sectorId > e.maxSector) continue;

                // once-per-run gate
                if (e.oncePerRun && persistent.HasUsedRandomEvent(e.data.EventId)) continue;

                // tags gate
                if (e.requiredTags != null)
                    foreach (var tag in e.requiredTags)
                        if (!persistent.HasStoryTag(tag)) goto skip;

                if (e.forbiddenTags != null)
                    foreach (var tag in e.forbiddenTags)
                        if (persistent.HasStoryTag(tag)) goto skip;

                // cooldown example (optional)
                // int last = persistent.GetEventLastSeenSector(e.data.EventId);
                // if (last >= 0 && (sectorId - last) < 2) continue;

                eligible.Add((e.data, Mathf.Max(1, e.weight)));
                continue;
                skip:;
            }

            if (eligible.Count == 0) return null;

            // weighted roll
            int total = 0; foreach (var it in eligible) total += it.weight;
            int r = UnityEngine.Random.Range(0, total);
            foreach (var it in eligible)
            {
                if (r < it.weight) return it.data;
                r -= it.weight;
            }
            return eligible[0].data;
        }

    }
}

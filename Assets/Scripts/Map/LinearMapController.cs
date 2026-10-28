using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// [MAP-DEMO-1] Linear demo map (D-MAP-1..3=A): a fixed sequence of nodes
    /// drawn over a hand-made map image — the band's last gigs before the
    /// Worm Queen. Thin by design: no generation, no conflicts, no random
    /// events. The procedural <see cref="SectorMapManager"/> stays untouched
    /// for Phase C.
    ///
    /// Flow:
    /// - Start: cohesion &lt;= 0 → Game Over (D-DIAG-1). Otherwise nodes before
    ///   <see cref="PersistentGameplayData.LinearMapNextNode"/> show as
    ///   completed, that node is the only clickable one, later nodes are
    ///   visible but locked (D-MAP-3: the whole map is visible).
    /// - Click on the active node → <see cref="GigLauncher.Launch"/> with that
    ///   node's <see cref="DemoLaunchConfigSO"/> (D-MAP-DEMO-4=A). The node is
    ///   consumed at launch: win or lose, the run continues on the next node
    ///   (D-MAP-DEMO-2=A). The band roster is applied on the first node only;
    ///   later nodes keep deck and SFX unlocks (preserveRunState, D-MAP-DEMO-1=A).
    /// - The gig returns here through <c>GigManager.ReturnToMap</c> →
    ///   <c>SceneChanger.OpenMapScene</c> (SceneData.sectorMapSceneIndex points
    ///   to this scene in the demo, D-MAP-DEMO-3=A). The last node is the boss:
    ///   <c>isFinalEncounter</c> = true, so its win ends on the WinPanel.
    /// </summary>
    public class LinearMapController : MonoBehaviour
    {
        private const string DebugTag = "[LinearMap]";

        [Serializable]
        public class LinearMapNode
        {
            [Tooltip("Node placed in the scene over the map image (NodePrefab).")]
            public SectorNodeVisual visual;

            [Tooltip("Encounter + tuning for this node. The roster is read on " +
                "the first node only.")]
            public DemoLaunchConfigSO launchConfig;

            public string title = "Gig";
            [TextArea] public string description;
        }

        [Header("Nodes (in play order; the last one is the boss)")]
        [SerializeField] private List<LinearMapNode> nodes = new List<LinearMapNode>();

        [Header("Ship")]
        [SerializeField] private ShipController shipPrefab;
        [SerializeField, Tooltip("Where the ship waits before the first node.")]
        private Transform startAnchor;

        [Header("Launch dependencies (same assets as MainMenuController)")]
        [SerializeField] private GigSetupRosterSO setupRoster;
        [SerializeField] private GigFlowSettingsSO flowSettings;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Look")]
        [SerializeField] private Color gigColor = Color.white;
        [SerializeField] private Color bossColor = new Color(1f, 0.45f, 0.45f);
        [SerializeField] private float nodeScale = 0.5f;
        [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.6f;
        [SerializeField] private float activePulseAmount = 0.12f;
        [SerializeField] private float activePulseSpeed = 3f;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI cohesionText;

        private ShipController _ship;
        private int _activeIndex = -1;
        private bool _launching;

        private PersistentGameplayData PD =>
            GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();
        }

        private void Start()
        {
            var pd = PD;
            if (pd == null)
            {
                Debug.LogError($"{DebugTag} PersistentGameplayData is null; map inert.");
                return;
            }

            if (cohesionText != null)
                cohesionText.text = $"Cohesión: {pd.BandCohesion}";

            // [D-DIAG-1] Cohesion 0 is the only defeat. A lost gig returns here
            // with the penalty already applied by GigManager.ReturnToMap.
            if (pd.BandCohesion <= 0)
            {
                Debug.Log($"{DebugTag} Cohesion={pd.BandCohesion} → Game Over.");
                if (sceneChanger != null) sceneChanger.OpenGameOverScene();
                return;
            }

            _activeIndex = pd.LinearMapNextNode;
            Debug.Log($"{DebugTag} Start | NextNode={_activeIndex}/{nodes.Count} | " +
                      $"Cohesion={pd.BandCohesion} | Fans={pd.Fans} | GigsWon={pd.GigsWon}");

            // Only reachable by losing the boss (a boss win ends on the WinPanel).
            // Provisional until COH-1 decides it (proposal: losing to the Queen
            // is game over).
            if (_activeIndex >= nodes.Count)
            {
                Debug.Log($"{DebugTag} All nodes consumed (boss lost) → Game Over.");
                if (sceneChanger != null) sceneChanger.OpenGameOverScene();
                return;
            }

            BindNodes();
            PlaceShip();
        }

        private void Update()
        {
            if (_activeIndex < 0 || _activeIndex >= nodes.Count) return;
            var v = nodes[_activeIndex].visual;
            if (v == null) return;
            float s = nodeScale * (1f + activePulseAmount * Mathf.Sin(Time.time * activePulseSpeed));
            v.transform.localScale = Vector3.one * s;
        }

        private void BindNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null || n.visual == null)
                {
                    Debug.LogError($"{DebugTag} Node {i} has no visual.");
                    continue;
                }

                bool isBoss = i == nodes.Count - 1;
                var state = new SectorNodeState
                {
                    Id = i,
                    Type = isBoss ? Enums.NodeType.Boss : Enums.NodeType.Gig,
                    Completed = i < _activeIndex,
                    Visited = i < _activeIndex,
                    Locked = i > _activeIndex,
                };

                n.visual.UseBoundTooltipOnly = true;
                n.visual.Bind(state, isBoss ? bossColor : gigColor,
                    n.title, n.description, sprite: null, scale: nodeScale);

                if (state.Completed) n.visual.SetCompleted(true);
                else if (state.Locked) SetAlpha(n.visual, lockedAlpha);

                n.visual.Clicked -= HandleNodeClicked;
                n.visual.Clicked += HandleNodeClicked;
            }
        }

        private void PlaceShip()
        {
            if (shipPrefab == null) return;

            // Ship sits on the last played node, or on the start anchor.
            Transform target = startAnchor;
            int last = _activeIndex - 1;
            if (last >= 0 && last < nodes.Count && nodes[last]?.visual != null)
                target = nodes[last].visual.transform;
            if (target == null) return;

            _ship = Instantiate(shipPrefab);
            _ship.AttachTo(target);
        }

        private void HandleNodeClicked(SectorNodeVisual visual)
        {
            if (_launching || visual == null || visual.Node == null) return;

            int index = visual.Node.Id;
            if (index != _activeIndex)
            {
                Debug.Log($"{DebugTag} Click on node {index} ignored (active={_activeIndex}).");
                return;
            }

            var node = nodes[index];
            var config = node.launchConfig;
            string reason = "launchConfig is null.";
            if (config == null || !config.IsValid(out reason))
            {
                Debug.LogError($"{DebugTag} Node {index} launch config invalid: {reason}");
                return;
            }

            var runConfig = config.ToRunConfig(GigRunContext.GigReturnDestination.Map);
            if (runConfig == null) return; // ToRunConfig already logged.

            bool first = index == 0;
            bool isBoss = index == nodes.Count - 1;
            runConfig.preserveRunState = !first;
            List<MusicianBase> roster = first ? config.ResolvedBandRoster : null;

            var pd = PD;
            int previous = pd.LinearMapNextNode;
            pd.LinearMapNextNode = index + 1; // consumed at launch (D-MAP-DEMO-2=A)

            Debug.Log($"{DebugTag} Launch node {index} '{node.title}' | " +
                      $"Encounter={config.Encounter.GetLabel()} | Boss={isBoss} | " +
                      $"PreserveRunState={runConfig.preserveRunState}");

            _launching = GigLauncher.Launch(
                runConfig: runConfig,
                bandRoster: roster,
                setupRoster: setupRoster,
                flowSettings: flowSettings,
                isFinalEncounter: isBoss,
                sceneChanger: sceneChanger,
                callerTag: "LinearMap");

            if (!_launching) pd.LinearMapNextNode = previous;
        }

        private static void SetAlpha(SectorNodeVisual v, float a)
        {
            var sr = v.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            var c = sr.color; c.a = a; sr.color = c;
        }
    }
}

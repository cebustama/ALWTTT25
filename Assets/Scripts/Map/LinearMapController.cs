using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System;
using System.Collections;
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

        // [MAP-DEMO-2] Gigs the band already played before the demo starts:
        // decor only (completed look, tooltip, no click), joined by the path.
        [Serializable]
        public class PastStop
        {
            public SectorNodeVisual visual;
            public string title = "Gig";
            [TextArea] public string description;
        }

        [Header("Nodes (in play order; the last one is the boss)")]
        [SerializeField] private List<LinearMapNode> nodes = new List<LinearMapNode>();

        [Header("Past gigs (in tour order, before the start anchor)")]
        [SerializeField] private List<PastStop> pastStops = new List<PastStop>();

        [Header("Boss preview (shown while hovering the last node)")]
        [SerializeField, Tooltip("UI object in a Screen Space canvas: portrait, " +
            "name, traits. Hidden at start; never blocks the pointer.")]
        private GameObject bossPreview;

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

        // [S5 NAVE-1] Where a gig launched from this map returns (D2=A: the
        // ship hub). Map keeps the MAP-DEMO-1 behaviour for test scenes.
        [Header("Return (S5)")]
        [SerializeField, Tooltip("Where the gig returns. Ship = ship hub " +
            "(demo flow, S5); Map = back to this map (MAP-DEMO-1).")]
        private GigRunContext.GigReturnDestination gigReturnDestination =
            GigRunContext.GigReturnDestination.Ship;

        // [MAP-DEMO-2] On the first node the camera starts over the part of the
        // map the band has already toured and eases to its scene position.
        [Header("Intro pan (first node only)")]
        [SerializeField] private bool playIntroPan = true;
        [SerializeField, Tooltip("Empty if Camera.main.")] private Camera mapCamera;
        [SerializeField, Tooltip("Where the camera starts (X/Y only). Its scene " +
            "position is where it ends.")]
        private Transform introPanStart;
        [SerializeField] private float introPanDelay = 0.4f;
        [SerializeField] private float introPanDuration = 3.5f;
        [SerializeField, Tooltip("X = time 0..1, Y = progress 0..1.")]
        private AnimationCurve introPanEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // [MAP-DEMO-2] Dotted path through the nodes, built from dot sprites.
        [Header("Path (dotted line)")]
        [SerializeField] private bool drawPath = true;
        [SerializeField] private bool pathFromStartAnchor = true;
        [SerializeField] private Color pathColor = new Color(1f, 0.3f, 0.3f, 0.9f);
        [SerializeField, Range(0f, 1f)] private float pathTraveledAlpha = 0.35f;
        [SerializeField] private float pathDotSize = 0.5f;
        [SerializeField] private float pathDotSpacing = 1.2f;
        [SerializeField, Tooltip("Free space left around each node (world units).")]
        private float pathNodeClearance = 1f;
        [SerializeField, Tooltip("Sorting order relative to the nodes (-1 = just behind).")]
        private int pathSortingOffset = -1;

        private static Sprite s_dotSprite;

        private ShipController _ship;
        private int _activeIndex = -1;
        private bool _launching;
        private bool _introPlaying;

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
            // [COH-1 / D1] The gig already sends a Queen loss to Game Over;
            // this stays as a safety net (e.g. Dev "force lose" → ReturnToMap).
            if (_activeIndex >= nodes.Count)
            {
                Debug.Log($"{DebugTag} All nodes consumed (boss lost) → Game Over.");
                if (sceneChanger != null) sceneChanger.OpenGameOverScene();
                return;
            }

            BindNodes();
            PlaceShip();
            if (drawPath) BuildPath();

            if (playIntroPan && _activeIndex == 0)
                StartCoroutine(IntroPan());
        }

        private IEnumerator IntroPan()
        {
            var cam = mapCamera != null ? mapCamera : Camera.main;
            if (cam == null || introPanStart == null) yield break;

            _introPlaying = true;
            var t = cam.transform;
            Vector3 end = t.position;
            Vector3 start = new Vector3(introPanStart.position.x, introPanStart.position.y, end.z);
            t.position = start;
            Debug.Log($"{DebugTag} Intro pan | {start.x:0.##} → {end.x:0.##} | {introPanDuration}s");

            float time = -introPanDelay;
            while (time < introPanDuration)
            {
                yield return null;
                if (Input.anyKeyDown) break; // any key or click skips
                time += Time.deltaTime;
                float u = Mathf.Clamp01(time / Mathf.Max(0.01f, introPanDuration));
                t.position = Vector3.LerpUnclamped(start, end, introPanEasing.Evaluate(u));
            }

            t.position = end;
            _introPlaying = false;
        }

        private void BuildPath()
        {
            // Path order: past gigs → start anchor → demo nodes. A segment is
            // "traveled" (dimmed) when the point it ends on was already reached.
            var points = new List<Transform>();
            var reached = new List<bool>();
            foreach (var p in pastStops)
                if (p?.visual != null) { points.Add(p.visual.transform); reached.Add(true); }
            if (pathFromStartAnchor && startAnchor != null) { points.Add(startAnchor); reached.Add(true); }
            for (int k = 0; k < nodes.Count; k++)
                if (nodes[k]?.visual != null) { points.Add(nodes[k].visual.transform); reached.Add(k < _activeIndex); }
            if (points.Count < 2) return;

            var reference = nodes[0]?.visual != null
                ? nodes[0].visual.GetComponent<SpriteRenderer>() : null;
            var root = new GameObject("Path").transform;
            root.SetParent(transform, false);
            var sprite = DotSprite();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 a = points[i].position, b = points[i + 1].position;
                Vector3 dir = b - a; dir.z = 0f;
                float usable = dir.magnitude - 2f * pathNodeClearance;
                if (usable <= 0f) continue;
                dir.Normalize();

                var color = pathColor;
                if (reached[i + 1]) color.a *= pathTraveledAlpha;

                int count = Mathf.Max(1, Mathf.FloorToInt(usable / pathDotSpacing) + 1);
                float step = count > 1 ? usable / (count - 1) : 0f;
                for (int d = 0; d < count; d++)
                {
                    var dot = new GameObject("Dot").AddComponent<SpriteRenderer>();
                    dot.transform.SetParent(root, false);
                    dot.transform.position = a + dir * (pathNodeClearance + d * step);
                    dot.transform.localScale = Vector3.one * pathDotSize;
                    dot.sprite = sprite;
                    dot.color = color;
                    if (reference != null)
                    {
                        // Same material as the nodes: the default one is lit by
                        // 2D lights and rendered the dots black.
                        dot.sharedMaterial = reference.sharedMaterial;
                        dot.sortingLayerID = reference.sortingLayerID;
                        dot.sortingOrder = reference.sortingOrder + pathSortingOffset;
                    }
                }
            }
        }

        // A soft round dot, generated once (no texture asset needed).
        private static Sprite DotSprite()
        {
            if (s_dotSprite != null) return s_dotSprite;
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - dist)));
                }
            tex.Apply();
            s_dotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return s_dotSprite;
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

                if (isBoss && bossPreview != null)
                {
                    n.visual.SuppressTooltip = true;
                    n.visual.HoverEnter += _ => bossPreview.SetActive(true);
                    n.visual.HoverExit += _ => bossPreview.SetActive(false);
                }
            }

            for (int i = 0; i < pastStops.Count; i++)
            {
                var p = pastStops[i];
                if (p?.visual == null) continue;
                var state = new SectorNodeState
                {
                    Id = -1 - i, // never matches _activeIndex: not clickable
                    Type = Enums.NodeType.Gig,
                    Completed = true,
                    Visited = true,
                };
                p.visual.UseBoundTooltipOnly = true;
                p.visual.Bind(state, gigColor, p.title, p.description, sprite: null, scale: nodeScale);
                p.visual.SetCompleted(true);
            }

            if (bossPreview != null)
            {
                // The preview must never steal the pointer from the node,
                // or hovering would flicker it on and off.
                var group = bossPreview.GetComponent<CanvasGroup>();
                if (group == null) group = bossPreview.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;
                group.interactable = false;
                bossPreview.SetActive(false);
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
            if (_launching || _introPlaying || visual == null || visual.Node == null) return;

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

            var runConfig = config.ToRunConfig(gigReturnDestination);
            if (runConfig == null) return; // ToRunConfig already logged.

            bool first = index == 0;
            bool isBoss = index == nodes.Count - 1;
            runConfig.preserveRunState = !first;

            var pd = PD;

            // [S6 NAVE-2] The first node plays the band picked in the dormitory
            // (D2=A); without a pick it falls back to the config roster.
            List<MusicianBase> roster = null;
            if (first)
            {
                bool chosen = pd.HasChosenBand;
                roster = chosen ? new List<MusicianBase>(pd.ChosenBandRoster)
                                : config.ResolvedBandRoster;
                runConfig.deckLabel = DeckLabel(roster);
                Debug.Log($"[S6] Map → Gig 1 | lineup={(chosen ? pd.ChosenLineupId : "config")} | " +
                          $"musicians={roster.Count} | deck={runConfig.deckLabel}");
            }

            int previous = pd.LinearMapNextNode;
            pd.LinearMapNextNode = index + 1; // consumed at launch (D-MAP-DEMO-2=A)

            Debug.Log($"{DebugTag} Launch node {index} '{node.title}' | " +
                      $"Encounter={config.Encounter.GetLabel()} | Boss={isBoss} | " +
                      $"PreserveRunState={runConfig.preserveRunState}");
            Debug.Log($"[S5] Map → Gig | node {index} '{node.title}' | " +
                      $"returns to {gigReturnDestination}");

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

        /// <summary>
        /// [S5 NAVE-1] Wire to the map's «Volver a la nave» button OnClick.
        /// Leaves the map without consuming a node.
        /// </summary>
        public void OnBackToShipPressed()
        {
            if (_launching || sceneChanger == null) return;
            _launching = true; // one transition per click
            Debug.Log($"[S5] Map → Ship | NextNode={_activeIndex}");
            sceneChanger.OpenShipScene();
        }

        // [S6 NAVE-2] Same format as DemoLaunchConfigSO.ToRunConfig, but from
        // the roster that actually plays.
        private static string DeckLabel(List<MusicianBase> roster)
        {
            var ids = new List<string>(roster.Count);
            foreach (var m in roster)
                if (m != null && m.MusicianCharacterData != null)
                    ids.Add(m.MusicianCharacterData.CharacterId);
            return ids.Count > 0 ? "<auto:" + string.Join("+", ids) + ">" : "<auto:<empty>>";
        }

        private static void SetAlpha(SectorNodeVisual v, float a)
        {
            var sr = v.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            var c = sr.color; c.a = a; sr.color = c;
        }
    }
}

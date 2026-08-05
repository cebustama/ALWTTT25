#if ALWTTT_DEV
using ALWTTT.Characters.Audience;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// Dev Mode entry point. Toggles an IMGUI overlay with F12.
    /// Phase 1: infinite turns + convinced-audience reset + forced hand reset
    /// between song cycles.
    /// Phase 2: card spawner tab hosted in the same overlay (see <see cref="DevCardCatalogueTab"/>).
    /// Compiles only when ALWTTT_DEV scripting define is active.
    /// </summary>
    public class DevModeController : MonoBehaviour
    {
        private const string Tag = "<color=lime>[DevMode]</color>";

        public static DevModeController Instance { get; private set; }

        // ---------------------------------------------------------------
        // Infinite turns
        // ---------------------------------------------------------------
        public static bool InfiniteTurnsEnabled { get; private set; }

        // ---------------------------------------------------------------
        // Overlay state
        // ---------------------------------------------------------------
        [SerializeField, Range(1f, 4f)]
        [Tooltip("IMGUI scale multiplier. 2.0 = double size. Inspector-tweakable.")]
        private float _overlayScale = 2.0f;

        [SerializeField]
        [Tooltip("Extra verbose logging for Dev Mode paths (recommended ON during Phase 1 smoke tests).")]
        private bool _verboseLogs = true;

        private bool _overlayVisible;
        private Rect _windowRect = new Rect(10, 10, 720, 380);
        private int _convincedResetCount;

        // Phase 2: tab selection. 0 = Infinite, 1 = Catalogue.
        private int _activeTab;
        private static readonly string[] TabNames =
            { "Infinite", "Catalogue", "Stats", "Audio Mix", "Composition" };

        // [CSV-3] Outer scroll for tab content. GUILayout.Window auto-grows to
        // fit content, so a tall tab (Composition, after the R2a section) pushed
        // the window past the screen bottom with no way to reach it. Bounding
        // the content to the visible height and scrolling the overflow fixes it
        // for every tab.
        private Vector2 _tabScroll;
        private bool _resizing;

        // ---------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log($"{Tag} DevModeController initialized. Press F12 to toggle overlay.");

            // [S5b / Item 5] Count normal-flow gig outcomes for the dev Stats tab.
            SensoryEventBus.Instance?.Subscribe<GigOutcomeEvent>(OnGigOutcome);

            // [TLM-1] Run telemetry logger (per-gig JSONL record; reads bus only).
            DevRunTelemetryLogger.Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                InfiniteTurnsEnabled = false;
            }

            // [S5b / Item 5]
            SensoryEventBus.Instance?.Unsubscribe<GigOutcomeEvent>(OnGigOutcome);

            // [TLM-1]
            DevRunTelemetryLogger.Shutdown();
        }

        // [S5b / Item 5] Records normal-flow outcomes only; the editor Debug context-menu
        // Win/Lose paths bypass GigOutcomeEvent and are intentionally not counted.
        private void OnGigOutcome(GigOutcomeEvent e) => DevGigOutcomeTracker.Record(e.Won);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _overlayVisible = !_overlayVisible;
                Debug.Log($"{Tag} Overlay {(_overlayVisible ? "ON" : "OFF")}");
            }
        }

        // ---------------------------------------------------------------
        // IMGUI
        // ---------------------------------------------------------------

        private void OnGUI()
        {
            if (!_overlayVisible) return;

            float scale = Mathf.Max(1f, _overlayScale);

            // Clamp window to screen, accounting for scaled footprint
            float scaledW = _windowRect.width * scale;
            float scaledH = _windowRect.height * scale;
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0, Screen.width - scaledW) / scale);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0, Screen.height - scaledH) / scale);

            // Scale the entire overlay uniformly.
            Matrix4x4 prev = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            _windowRect = GUILayout.Window(
                99990, _windowRect, DrawWindow, "DEV MODE  (F12 to hide)");

            GUI.matrix = prev;
        }

        private void DrawWindow(int id)
        {
            // ---- Tab bar (stays fixed above the scroll) ----
            _activeTab = GUILayout.Toolbar(_activeTab, TabNames);
            GUILayout.Space(4);

            // [CSV-3] Bound tab content to the visible screen and scroll the
            // overflow. Coordinates here are logical (pre-scale), so the
            // available height is the physical screen height divided by scale,
            // minus the window's top offset and a reserve for the title + tab
            // bar. Clamped so it stays usable on small screens.
            float scale = Mathf.Max(1f, _overlayScale);
            float available = Mathf.Max(
                160f, (Screen.height / scale) - _windowRect.y - 72f);

            _tabScroll = GUILayout.BeginScrollView(
                _tabScroll, GUILayout.Height(available));

            switch (_activeTab)
            {
                case 0:
                    DrawInfiniteTab();
                    break;
                case 1:
                    DevCardCatalogueTab.Draw();
                    break;
                case 2:
                    DevStatsTab.Draw();
                    break;
                case 3:
                    DevAudioMixTab.Draw();
                    break;
                case 4:
                    DevCompositionDebugTab.Draw();   // [DBG-C1]
                    break;
            }

            GUILayout.EndScrollView();

            // Resize grip: the composition tab's lines are long (bundle names,
            // hashes, warnings) and a fixed 480 truncated them mid-sentence.
            // Width only — GUILayout.Window owns the height.
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                var grip = GUILayoutUtility.GetRect(18f, 18f, GUILayout.ExpandWidth(false));
                GUI.Label(grip, "◢");
                var e = Event.current;
                if (e.type == EventType.MouseDown && grip.Contains(e.mousePosition))
                { _resizing = true; e.Use(); }
                else if (_resizing && e.type == EventType.MouseDrag)
                { _windowRect.width = Mathf.Clamp(e.mousePosition.x + 10f, 380f, 2000f); e.Use(); }
                else if (e.type == EventType.MouseUp) { _resizing = false; }
            }

            // Drag by the title bar only, so click-drags inside the scrolled
            // content don't move the window (they scroll / hit controls).
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        }

        private void DrawInfiniteTab()
        {
            // ---- Infinite turns toggle ----
            bool prev = InfiniteTurnsEnabled;
            InfiniteTurnsEnabled = GUILayout.Toggle(InfiniteTurnsEnabled,
                " Infinite Turns (keep playing: new song each cycle, reset convinced)");

            if (InfiniteTurnsEnabled != prev)
                Debug.Log($"{Tag} Infinite turns → {InfiniteTurnsEnabled}");

            if (InfiniteTurnsEnabled)
            {
                GUILayout.Space(4);

                var style = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Italic,
                    fontSize = 11
                };
                GUILayout.Label(
                    "Gig will not end. New song begins each cycle. Convinced audience resets at each PlayerTurn.",
                    style);

                GUILayout.Space(4);

                if (GUILayout.Button("Reset Convinced Audience Now"))
                {
                    int count = ResetConvincedAudience();
                    Debug.Log($"{Tag} Manual reset: {count} audience member(s) un-convinced.");
                }

                GUILayout.Label($"  Auto-resets this gig: {_convincedResetCount}");
            }

            GUILayout.Space(12);

            // ---- Status readout ----
            var gm = GigManager.Instance;
            if (gm != null)
            {
                var pd = GameManager.Instance?.PersistentGameplayData;
                int songIndex = pd?.CurrentSongIndex ?? 0;
                int required = gm.RequiredSongCount;
                int cohesion = pd?.BandCohesion ?? 0;

                GUILayout.Label($"Song: {songIndex} / {required}   |   Cohesion: {cohesion}");

                // Extra diagnostic readouts (visible when verbose)
                if (_verboseLogs)
                {
                    int handCount = DeckManager.Instance?.HandController?.Hand?.Count ?? -1;
                    int drawPile = DeckManager.Instance?.DrawPile?.Count ?? -1;
                    int discardPile = DeckManager.Instance?.DiscardPile?.Count ?? -1;
                    int handPile = DeckManager.Instance?.HandPile?.Count ?? -1;
                    GUILayout.Label($"Hand: {handCount}  HandPile: {handPile}  Draw: {drawPile}  Discard: {discardPile}");
                    GUILayout.Label($"Phase: {gm.CurrentGigPhase}");

                    // M4.5 — last-turn guarantee summary. Always visible (cheap one-liner),
                    // not gated by _verboseLogs since it is a single line.
                    var dm = DeckManager.Instance;
                    if (dm != null)
                    {
                        var m45 = dm.LastTurnGuaranteeSummary;
                        if (!string.IsNullOrEmpty(m45))
                            GUILayout.Label($"M4.5 last draw: {m45}");
                    }
                }
            }

            // ---- SongHype controls [B2 / #6] ----
            GUILayout.Space(12);
            DrawSongHypeControls();

            GUILayout.Space(6);
            GUILayout.Label("Gig Outcome (dev)");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("WIN (rewards)"))
                    GigManager.Instance?.DevWinNormalFlow();
                if (GUILayout.Button("LOSE"))
                    GigManager.Instance?.DevLoseNormalFlow();
            }
        }

        /// <summary>
        /// [B2 / #6] Dev controls for SongHype: ±10% (relative to MaxSongHype)
        /// + Reset. Routes through GigManager.DevAddSongHype / DevResetSongHype
        /// so threshold-crossing venue SFX fire on upward steps and the per-song
        /// stage counter resets on Reset.
        /// </summary>
        private void DrawSongHypeControls()
        {
            var gm = GigManager.Instance;
            if (gm == null)
            {
                GUILayout.Label("SongHype controls: (no GigManager)");
                return;
            }

            float maxHype = gm.MaxSongHype;
            float pct = gm.SongHype01 * 100f;

            GUILayout.Label("SongHype controls:");
            GUILayout.Label($"  Current: {pct:0.0}%   ({gm.SongHype:0.0} / {maxHype:0.0})");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+10%"))
            {
                float delta = maxHype * 0.1f;
                gm.DevAddSongHype(delta);
                if (_verboseLogs)
                    Debug.Log($"{Tag} DevAddSongHype(+{delta:0.0}). " +
                        $"Now {gm.SongHype01 * 100f:0.0}%.");
            }
            if (GUILayout.Button("-10%"))
            {
                float delta = -maxHype * 0.1f;
                gm.DevAddSongHype(delta);
                if (_verboseLogs)
                    Debug.Log($"{Tag} DevAddSongHype({delta:0.0}). " +
                        $"Now {gm.SongHype01 * 100f:0.0}%.");
            }
            if (GUILayout.Button("Reset"))
            {
                gm.DevResetSongHype();
                if (_verboseLogs)
                    Debug.Log($"{Tag} DevResetSongHype. SongHype + stage counter reset.");
            }
            GUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------------
        // Infinite turns — convinced audience reset
        // ---------------------------------------------------------------

        /// <summary>
        /// Resets all convinced audience members: Vibe → 0, IsConvinced → false,
        /// clears Convinced legacy status. Returns how many were reset.
        /// Called automatically at each PlayerTurn start when infinite mode is on.
        /// </summary>
        public int ResetConvincedAudience()
        {
            var gm = GigManager.Instance;
            if (gm == null) return 0;

            int count = 0;
            foreach (var a in gm.CurrentAudienceCharacterList)
            {
                if (a == null) continue;
                if (!a.AudienceStats.IsConvinced) continue;

                a.Stats.DevResetConvinced();
                count++;

                Debug.Log($"{Tag} Reset convinced: {a.CharacterId}");
            }

            if (count > 0)
            {
                _convincedResetCount += count;
                gm.RecalculateAudienceObstructions();
            }

            return count;
        }

        /// <summary>
        /// Called from GigManager at PlayerTurn start when infinite mode is on.
        /// </summary>
        public void OnPlayerTurnStartInfiniteMode()
        {
            if (_verboseLogs)
                Debug.Log($"{Tag} >>> OnPlayerTurnStartInfiniteMode CALLED. " +
                          $"CurrentSongIndex={GameManager.Instance?.PersistentGameplayData?.CurrentSongIndex} " +
                          $"Required={GigManager.Instance?.RequiredSongCount} " +
                          $"InfiniteTurnsEnabled={InfiniteTurnsEnabled}");

            int count = ResetConvincedAudience();
            if (count > 0)
                Debug.Log($"{Tag} PlayerTurn auto-reset: {count} audience member(s) un-convinced.");
            else if (_verboseLogs)
                Debug.Log($"{Tag} PlayerTurn auto-reset: no convinced audience members to reset.");
        }
    }
}
#endif
#if ALWTTT_DEV
using ALWTTT.Characters.Audience;
using ALWTTT.Managers;
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
        // Phase 2: grown to 480x380 to fit the catalogue list comfortably at any scale.
        private Rect _windowRect = new Rect(10, 10, 480, 380);
        private int _convincedResetCount;

        // Phase 2: tab selection. 0 = Infinite, 1 = Catalogue.
        private int _activeTab;
        private static readonly string[] TabNames = { "Infinite", "Catalogue", "Stats" };

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
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                InfiniteTurnsEnabled = false;
            }
        }

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
            // ---- Tab bar ----
            _activeTab = GUILayout.Toolbar(_activeTab, TabNames);
            GUILayout.Space(4);

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
            }

            GUI.DragWindow();
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
                }
            }
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
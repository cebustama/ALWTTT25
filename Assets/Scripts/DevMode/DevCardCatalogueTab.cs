#if ALWTTT_DEV
using System.Collections.Generic;
using ALWTTT.Cards;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// Dev Mode Phase 2 catalogue tab. Draws a filterable list of every
    /// CardDefinition in GameplayData.AllCardsList and spawns the selected
    /// card into the player's hand through the normal play pipeline via
    /// <see cref="DeckManager.DevSpawnCardToHand(CardDefinition)"/>.
    ///
    /// Runtime-only: no AssetDatabase access.
    /// Compiles only when ALWTTT_DEV is defined.
    /// </summary>
    internal static class DevCardCatalogueTab
    {
        // Filter state (static so it persists while the overlay stays open).
        private static string _search = string.Empty;
        private static bool _filterAction = true;
        private static bool _filterComposition = true;
        private static Vector2 _scroll;

        // Cached filtered result so we don't reallocate on every OnGUI pass.
        private static readonly List<CardDefinition> _filtered = new List<CardDefinition>();
        private static int _lastSourceCount;
        private static string _lastSearch;
        private static bool _lastFilterAction;
        private static bool _lastFilterComposition;

        public static void Draw()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.GameplayData == null)
            {
                GUILayout.Label("GameManager.GameplayData is null.");
                return;
            }

            var all = gm.GameplayData.AllCardsList;
            if (all == null || all.Count == 0)
            {
                GUILayout.Label("GameplayData.AllCardsList is empty.");
                return;
            }

            DrawFilterRow();
            RefreshFilterIfDirty(all);
            DrawGateStatus(all.Count);
            DrawCardList();
        }

        // ---------------------------------------------------------------
        // Filter row
        // ---------------------------------------------------------------

        private static void DrawFilterRow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(55));
            _search = GUILayout.TextField(_search ?? string.Empty, GUILayout.MinWidth(120));
            _filterAction = GUILayout.Toggle(_filterAction, " Action", GUILayout.Width(70));
            _filterComposition = GUILayout.Toggle(_filterComposition, " Composition", GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------------
        // Gate status line
        // ---------------------------------------------------------------

        private static void DrawGateStatus(int sourceCount)
        {
            // Split cleanly so 'reason' is definitely assigned on both branches.
            bool canSpawn;
            string reason;

            if (DeckManager.Instance == null)
            {
                canSpawn = false;
                reason = "DeckManager.Instance is null";
            }
            else
            {
                canSpawn = DeckManager.Instance.CanDevSpawnToHand(out reason);
            }

            int handCount = DeckManager.Instance?.HandPile?.Count ?? -1;
            int maxHand = GameManager.Instance?.GameplayData != null
                ? GameManager.Instance.GameplayData.MaxCardsOnHand
                : -1;

            string status = canSpawn
                ? $"Ready. Hand: {handCount}/{maxHand}   ({_filtered.Count}/{sourceCount} shown)"
                : $"Spawn gated: {reason}   ({_filtered.Count}/{sourceCount} shown)";

            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Italic,
                fontSize = 11,
                wordWrap = true
            };
            GUILayout.Label(status, style);
        }

        // ---------------------------------------------------------------
        // Card list
        // ---------------------------------------------------------------

        private static void DrawCardList()
        {
            // Evaluate the gate once per frame so every row uses the same answer.
            bool canSpawn = DeckManager.Instance != null &&
                            DeckManager.Instance.CanDevSpawnToHand();

            _scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _filtered.Count; i++)
            {
                var def = _filtered[i];
                if (def == null) continue;

                GUILayout.BeginHorizontal();

                string badge = def.IsAction ? "[A]" : def.IsComposition ? "[C]" : "[?]";
                GUILayout.Label(badge, GUILayout.Width(28));

                string name = string.IsNullOrEmpty(def.DisplayName) ? def.name : def.DisplayName;
                GUILayout.Label($"{name}  (cost {def.InspirationCost})", GUILayout.ExpandWidth(true));

                GUI.enabled = canSpawn;
                if (GUILayout.Button("Spawn", GUILayout.Width(60)))
                {
                    DeckManager.Instance.DevSpawnCardToHand(def);
                    // Hand count just changed; gate may have flipped. Re-evaluate for the
                    // remainder of the frame so we don't visually "over-spawn" in one pass.
                    canSpawn = DeckManager.Instance != null &&
                               DeckManager.Instance.CanDevSpawnToHand();
                }
                GUI.enabled = true;

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        // ---------------------------------------------------------------
        // Filter refresh
        // ---------------------------------------------------------------

        private static void RefreshFilterIfDirty(IReadOnlyList<CardDefinition> source)
        {
            bool dirty =
                source.Count != _lastSourceCount ||
                _search != _lastSearch ||
                _filterAction != _lastFilterAction ||
                _filterComposition != _lastFilterComposition;

            if (!dirty) return;

            _filtered.Clear();
            string query = (_search ?? string.Empty).Trim();
            bool hasQuery = query.Length > 0;

            for (int i = 0; i < source.Count; i++)
            {
                var def = source[i];
                if (def == null) continue;

                bool kindOk =
                    (def.IsAction && _filterAction) ||
                    (def.IsComposition && _filterComposition);
                if (!kindOk) continue;

                if (hasQuery)
                {
                    string name = string.IsNullOrEmpty(def.DisplayName) ? def.name : def.DisplayName;
                    if (name == null ||
                        name.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                _filtered.Add(def);
            }

            _lastSourceCount = source.Count;
            _lastSearch = _search;
            _lastFilterAction = _filterAction;
            _lastFilterComposition = _filterComposition;
        }
    }
}
#endif
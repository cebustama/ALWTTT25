#if ALWTTT_DEV
using ALWTTT.Characters.Band;
using ALWTTT.Managers;
using ALWTTT.Status.Runtime;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// Dev Mode Stats tab — IMGUI helper for DevModeController.
    /// P3.1: Breakdown section (musician selector + force button + status readout).
    /// P3.2/P3.3 will add gig-wide and per-character stat editing sections.
    /// </summary>
    public static class DevStatsTab
    {
        private static int _selectedMusicianIndex;
        private static Vector2 _scrollPos;
        private static GUIStyle _sectionHeader;

        public static void Draw()
        {
            var gm = GigManager.Instance;
            if (gm == null)
            {
                GUILayout.Label("GigManager not available.");
                return;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            DrawBreakdownSection(gm);
            // P3.2: DrawGigWideSection(gm);
            // P3.3: DrawPerCharacterSection(gm);
            GUILayout.EndScrollView();
        }

        private static void DrawBreakdownSection(GigManager gm)
        {
            if (_sectionHeader == null)
                _sectionHeader = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Bold, fontSize = 13 };

            GUILayout.Label("── Breakdown ──", _sectionHeader);

            var musicians = gm.CurrentMusicianCharacterList;
            if (musicians == null || musicians.Count == 0)
            {
                GUILayout.Label("No musicians spawned.");
                return;
            }

            // Build label array for selection grid
            var names = new string[musicians.Count];
            for (int i = 0; i < musicians.Count; i++)
            {
                var m = musicians[i];
                if (m == null) { names[i] = "(null)"; continue; }
                var s = m.Stats;
                string stress = s != null
                    ? $" [{s.CurrentStress}/{s.MaxStress}]"
                    : "";
                string bd = (s != null && s.IsBreakdown) ? " (BD)" : "";
                names[i] = $"{m.CharacterName}{stress}{bd}";
            }

            _selectedMusicianIndex = Mathf.Clamp(
                _selectedMusicianIndex, 0, musicians.Count - 1);
            _selectedMusicianIndex = GUILayout.SelectionGrid(
                _selectedMusicianIndex, names, Mathf.Min(musicians.Count, 3));

            GUILayout.Space(4);

            var selected = musicians[_selectedMusicianIndex];
            if (selected == null) { GUILayout.Label("(null)"); return; }

            // Status readout
            var stats = selected.Stats;
            if (stats != null)
            {
                GUILayout.Label(
                    $"Stress: {stats.CurrentStress}/{stats.MaxStress}  " +
                    $"Breakdown: {stats.IsBreakdown}  Stunned: {selected.IsStunned}");
            }

            var container = selected.Statuses;
            if (container != null && container.Active.Count > 0)
            {
                var sb = new System.Text.StringBuilder("Statuses: ");
                foreach (var kvp in container.Active)
                {
                    var inst = kvp.Value;
                    if (inst != null && inst.Stacks > 0)
                        sb.Append($"{inst.Definition.DisplayName}×{inst.Stacks}  ");
                }
                GUILayout.Label(sb.ToString());
            }

            GUILayout.Space(4);

            if (GUILayout.Button($"Force Breakdown → {selected.CharacterName}"))
            {
                selected.DevForceBreakdown();
            }

            GUILayout.Space(8);
        }
    }
}
#endif
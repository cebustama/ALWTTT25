#if ALWTTT_DEV
using ALWTTT.Characters;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// Dev Mode Stats tab — IMGUI helper for DevModeController.
    /// P3.1: Breakdown section (musician selector + force button + status readout).
    /// P3.2: Gig-wide stat editing (SongHype, Inspiration, Cohesion).
    /// P3.3a: Per-character stat editing (musician Stress/MaxStress/Composure,
    ///        audience Vibe/MaxVibe) + Flow added to gig-wide section.
    /// </summary>
    public static class DevStatsTab
    {
        private static int _selectedMusicianIndex;
        private static int _selectedAudienceIndex;
        private static Vector2 _scrollPos;
        private static GUIStyle _sectionHeader;

        private static int _musicianStatusPickerIndex;
        private static int _audienceStatusPickerIndex;

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
            DrawGigWideSection(gm);
            DrawPerCharacterSection(gm);
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

        private static void DrawGigWideSection(GigManager gm)
        {
            if (_sectionHeader == null)
                _sectionHeader = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Bold, fontSize = 13 };

            GUILayout.Label("── Gig-Wide Stats ──", _sectionHeader);

            var pd = GameManager.Instance?.PersistentGameplayData;
            if (pd == null)
            {
                GUILayout.Label("PersistentGameplayData not available.");
                return;
            }

            // --- SongHype slider (float) ---
            float currentHype = gm.SongHype;
            float maxHype = gm.MaxSongHype;
            GUILayout.BeginHorizontal();
            GUILayout.Label("SongHype:", GUILayout.Width(100));
            float newHype = GUILayout.HorizontalSlider(
                currentHype, 0f, maxHype, GUILayout.ExpandWidth(true));
            GUILayout.Label($"{currentHype:0.0}/{maxHype:0}", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newHype - currentHype) > 0.01f)
                gm.DevSetSongHype(newHype);

            // --- Inspiration slider (int) ---
            int currentInsp = pd.CurrentInspiration;
            int maxInsp = pd.MaxInspiration;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Inspiration:", GUILayout.Width(100));
            float newInspF = GUILayout.HorizontalSlider(
                currentInsp, 0f, Mathf.Max(1, maxInsp), GUILayout.ExpandWidth(true));
            int newInsp = Mathf.RoundToInt(newInspF);
            GUILayout.Label($"{currentInsp}/{maxInsp}", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (newInsp != currentInsp)
                gm.DevSetInspiration(newInsp);

            // --- BandCohesion stepper (int, no upper cap) ---
            int cohesion = pd.BandCohesion;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cohesion:", GUILayout.Width(100));
            if (GUILayout.Button("−", GUILayout.Width(30)))
                gm.DevSetBandCohesion(cohesion - 1);
            GUILayout.Label($"{cohesion}", GUILayout.Width(40));
            if (GUILayout.Button("+", GUILayout.Width(30)))
                gm.DevSetBandCohesion(cohesion + 1);
            GUILayout.EndHorizontal();

            // --- Flow stepper (applies ±1 to every musician's DamageUpFlat) ---
            // Flow is song/band-scoped: aggregate display = sum across musicians.
            // Editing is uniform — one stepper press = ±1 stack to each musician.
            // Authored MaxStacks on the Flow SO is respected by StatusEffectContainer.
            int totalFlow = gm.TotalFlowStacks;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Flow (all):", GUILayout.Width(100));
            if (GUILayout.Button("−", GUILayout.Width(30)))
                gm.DevAddFlowToAllMusicians(-1);
            GUILayout.Label($"{totalFlow}", GUILayout.Width(40));
            if (GUILayout.Button("+", GUILayout.Width(30)))
                gm.DevAddFlowToAllMusicians(1);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
        }

        private static void DrawPerCharacterSection(GigManager gm)
        {
            if (_sectionHeader == null)
                _sectionHeader = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Bold, fontSize = 13 };

            GUILayout.Label("── Per-Character ──", _sectionHeader);

            DrawMusicianEditor(gm);
            GUILayout.Space(6);
            DrawAudienceEditor(gm);
            GUILayout.Space(8);
        }

        private static void DrawMusicianEditor(GigManager gm)
        {
            GUILayout.Label("Musician:");

            var musicians = gm.CurrentMusicianCharacterList;
            if (musicians == null || musicians.Count == 0)
            {
                GUILayout.Label("  No musicians spawned.");
                return;
            }

            // Shares _selectedMusicianIndex with Breakdown section — the two selectors
            // stay in sync. Slight redundancy is acceptable; keeps both sections stateful.
            _selectedMusicianIndex = Mathf.Clamp(
                _selectedMusicianIndex, 0, musicians.Count - 1);
            var names = new string[musicians.Count];
            for (int i = 0; i < musicians.Count; i++)
            {
                var m = musicians[i];
                names[i] = m != null ? m.CharacterName : "(null)";
            }
            _selectedMusicianIndex = GUILayout.SelectionGrid(
                _selectedMusicianIndex, names, Mathf.Min(musicians.Count, 3));

            var selected = musicians[_selectedMusicianIndex];
            if (selected == null) { GUILayout.Label("  (null)"); return; }
            var stats = selected.Stats;
            if (stats == null) { GUILayout.Label("  (no stats)"); return; }

            // --- CurrentStress slider ---
            // DevSetCurrentStress fires Breakdown at >= MaxStress (sticky).
            int currentStress = stats.CurrentStress;
            int maxStress = Mathf.Max(1, stats.MaxStress);
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Stress:", GUILayout.Width(100));
            float newStressF = GUILayout.HorizontalSlider(
                currentStress, 0f, maxStress, GUILayout.ExpandWidth(true));
            int newStress = Mathf.RoundToInt(newStressF);
            GUILayout.Label($"{currentStress}/{maxStress}", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (newStress != currentStress)
                stats.DevSetCurrentStress(newStress);

            // --- MaxStress stepper (floor 1) ---
            // DevSetMaxStress clamps CurrentStress down and re-checks Breakdown.
            GUILayout.BeginHorizontal();
            GUILayout.Label("  MaxStress:", GUILayout.Width(100));
            if (GUILayout.Button("−", GUILayout.Width(30)))
                stats.DevSetMaxStress(maxStress - 1);
            GUILayout.Label($"{maxStress}", GUILayout.Width(40));
            if (GUILayout.Button("+", GUILayout.Width(30)))
                stats.DevSetMaxStress(maxStress + 1);
            GUILayout.EndHorizontal();

            // --- Composure stepper (TempShieldTurn stacks on StatusEffectContainer) ---
            // Authored MaxStacks on the Composure SO is respected by the container's
            // stacking policy. Below 0 is guarded by disabling the − button.
            int composure = selected.Statuses != null
                ? selected.Statuses.GetStacks(CharacterStatusId.TempShieldTurn)
                : 0;
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Composure:", GUILayout.Width(100));
            GUI.enabled = composure > 0;
            if (GUILayout.Button("−", GUILayout.Width(30)))
                ApplyComposureDelta(selected, -1);
            GUI.enabled = true;
            GUILayout.Label($"{composure}", GUILayout.Width(40));
            if (GUILayout.Button("+", GUILayout.Width(30)))
                ApplyComposureDelta(selected, 1);
            GUILayout.EndHorizontal();
            DrawStatusPicker(selected, ref _musicianStatusPickerIndex);
        }

        /// <summary>
        /// Resolve the Composure StatusEffectSO via the selected musician's
        /// catalogue and apply the delta. No-op if catalogue or key missing.
        /// </summary>
        private static void ApplyComposureDelta(MusicianBase musician, int delta)
        {
            if (musician == null || musician.Statuses == null) return;
            if (musician.StatusCatalogue == null) return;
            if (!musician.StatusCatalogue.TryGetByKey("composure", out var composureSO)
                || composureSO == null)
                return;
            musician.Statuses.Apply(composureSO, delta);
        }

        private static void DrawAudienceEditor(GigManager gm)
        {
            GUILayout.Label("Audience:");

            var audienceList = gm.CurrentAudienceCharacterList;
            if (audienceList == null || audienceList.Count == 0)
            {
                GUILayout.Label("  No audience spawned.");
                return;
            }

            _selectedAudienceIndex = Mathf.Clamp(
                _selectedAudienceIndex, 0, audienceList.Count - 1);
            var names = new string[audienceList.Count];
            for (int i = 0; i < audienceList.Count; i++)
            {
                var a = audienceList[i];
                if (a == null) { names[i] = "(null)"; continue; }
                // AudienceCharacterBase exposes display name via AudienceCharacterData,
                // not via a CharacterName property (asymmetric with MusicianBase).
                string name = a.AudienceCharacterData != null
                    ? a.AudienceCharacterData.CharacterName
                    : "(no data)";
                var s = a.Stats;
                string vibe = s != null ? $" [{s.CurrentVibe}/{s.MaxVibe}]" : "";
                string conv = (s != null && s.IsConvinced) ? " (C)" : "";
                names[i] = $"{name}{vibe}{conv}";
            }
            _selectedAudienceIndex = GUILayout.SelectionGrid(
                _selectedAudienceIndex, names, Mathf.Min(audienceList.Count, 3));

            var selected = audienceList[_selectedAudienceIndex];
            if (selected == null) { GUILayout.Label("  (null)"); return; }
            var stats = selected.Stats;
            if (stats == null) { GUILayout.Label("  (no stats)"); return; }

            // --- CurrentVibe slider ---
            // DevSetCurrentVibe fires Convinced at >= MaxVibe (sticky).
            int currentVibe = stats.CurrentVibe;
            int maxVibe = Mathf.Max(1, stats.MaxVibe);
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Vibe:", GUILayout.Width(100));
            float newVibeF = GUILayout.HorizontalSlider(
                currentVibe, 0f, maxVibe, GUILayout.ExpandWidth(true));
            int newVibe = Mathf.RoundToInt(newVibeF);
            GUILayout.Label($"{currentVibe}/{maxVibe}", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (newVibe != currentVibe)
                stats.DevSetCurrentVibe(newVibe);

            // --- MaxVibe stepper (floor 1) ---
            GUILayout.BeginHorizontal();
            GUILayout.Label("  MaxVibe:", GUILayout.Width(100));
            if (GUILayout.Button("−", GUILayout.Width(30)))
                stats.DevSetMaxVibe(maxVibe - 1);
            GUILayout.Label($"{maxVibe}", GUILayout.Width(40));
            if (GUILayout.Button("+", GUILayout.Width(30)))
                stats.DevSetMaxVibe(maxVibe + 1);
            GUILayout.EndHorizontal();
            DrawStatusPicker(selected, ref _audienceStatusPickerIndex);
        }

        private static void DrawStatusPicker(CharacterBase character, ref int pickerIndex)
        {
            GUILayout.Label("  Statuses:");

            var container = character.Statuses;
            if (container == null)
            {
                GUILayout.Label("    (no container)");
                return;
            }

            // --- Active statuses with −1 / Clear ---
            if (container.Active.Count > 0)
            {
                var keys = new System.Collections.Generic.List<CharacterStatusId>(container.Active.Keys);

                foreach (var id in keys)
                {
                    if (!container.Active.TryGetValue(id, out var inst) || inst == null || inst.Stacks <= 0)
                        continue;

                    string name = inst.Definition != null
                        ? inst.Definition.DisplayName
                        : id.ToString();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"    {name} ×{inst.Stacks}", GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("−1", GUILayout.Width(30)))
                        container.Apply(inst.Definition, -1);
                    if (GUILayout.Button("Clear", GUILayout.Width(50)))
                        container.Clear(id);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("    (none active)");
            }

            // --- Catalogue picker for Apply ---
            var catalogue = character.StatusCatalogue;
            if (catalogue == null || catalogue.Effects == null || catalogue.Effects.Count == 0)
            {
                GUILayout.Label("    (no catalogue — assign on prefab)");
                return;
            }

            var effects = catalogue.Effects;
            var valid = new System.Collections.Generic.List<StatusEffectSO>(effects.Count);
            for (int i = 0; i < effects.Count; i++)
                if (effects[i] != null) valid.Add(effects[i]);

            if (valid.Count == 0) return;

            pickerIndex = Mathf.Clamp(pickerIndex, 0, valid.Count - 1);
            var selected = valid[pickerIndex];
            string displayLabel = string.IsNullOrWhiteSpace(selected.DisplayName)
                ? selected.name
                : $"{selected.DisplayName} ({selected.EffectId})";

            GUILayout.BeginHorizontal();
            GUILayout.Label("  Apply:", GUILayout.Width(60));
            if (GUILayout.Button("◄", GUILayout.Width(25)))
                pickerIndex = (pickerIndex - 1 + valid.Count) % valid.Count;
            GUILayout.Label(displayLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("►", GUILayout.Width(25)))
                pickerIndex = (pickerIndex + 1) % valid.Count;
            if (GUILayout.Button("+1", GUILayout.Width(30)))
            {
                container.Apply(selected, 1);
                Debug.Log($"<color=lime>[DevMode]</color> StatusPicker: Applied " +
                          $"{selected.DisplayName} ×1 to {character.name}. " +
                          $"Stacks now: {container.GetStacks(selected.EffectId)}");
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif
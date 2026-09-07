// GigEncounterWizard.cs — GEW-1 (2026-09-06)
// Repo path: Assets/Scripts/Encounters/Editor/GigEncounterWizard.cs
// Editor-only authoring window for GigEncounterSO. No runtime type is touched (D-GEW-0).
// Authority once applied: SSoT_Editor_Authoring_Tools.md §22.
//
// Rules honoured (Design_GigEncounterWizard_Requirements_v0_1 §6, all mandatory):
//  1. One row = one rect + shared BuildCols; never a HorizontalScope per row.
//  2. Numbers right, flags/short labels centred, text left.
//  3. Absent dimmed '·', present '✓'; defaults dimmed.
//  4. Zebra striping, whole-row click selects, double click pings.
//  5. Row badges never repeat a column; badges live in the panel. Rows carry a '!' count column.
//  6. Header outside the scroll, same grid, 1 px rule under it.
//  7. Counters are one label with a tooltip.
//  8. Structural list edits: ApplyModifiedProperties → refresh derived → GUIUtility.ExitGUI().
//  9. arraySize++ copies the previous element → explicit reset (null) for the new entry.
// 10. Every derived column is read from the SerializedObject stream, never from public getters.
// 11. Window min width is computed from the panels; fixed column widths sum below each panel min.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Characters.Editor; // AudienceMemberWizard.OpenAndSelect (AMW-1 entry point)

namespace ALWTTT.Encounters.Editor
{
    public sealed class GigEncounterWizard : EditorWindow
    {
        // ─────────────────────────────────────────────────────────────── constants
        const float RowH = 20f;
        const float HeaderH = 20f;
        const float Pad = 4f;
        const float LeftMin = 500f;   // inventory: fixed 376 + 8 pads (32) + Name min 90 = 498 ≤ LeftMin (rule 11)
        const float RightMin = 500f;  // member row: fixed 272 + 11 pads (44) + flex mins 180 = 496 ≤ RightMin (rule 11)
        const float SplitterW = 4f;

        // Serialized field names read from the stream (rule 10). A missing name → red banner.
        const string F_Venue = "targetVenueType";
        const string F_DisplayName = "displayName";
        const string F_Audience = "audienceMemberList";
        const string F_Songs = "numberOfSongs";
        const string F_Fans = "fansOnWin";
        const string F_CohLoss = "cohesionPenaltyOnLoss";
        static readonly string[] EncounterFields = { F_Venue, F_DisplayName, F_Audience, F_Songs, F_Fans, F_CohLoss };

        const string M_Id = "characterId";
        const string M_Name = "characterName";
        const string M_Vibe = "maxVibe";
        const string M_Prefab = "characterPrefab";
        const string M_Abilities = "abilityList";
        const string M_Pattern = "followAbilityPattern";
        const string M_Taste = "taste";
        static readonly string[] MemberFields = { M_Id, M_Name, M_Vibe, M_Prefab, M_Abilities, M_Pattern, M_Taste };

        const string R_Encounters = "availableEncounters";
        const string R_MaxAudience = "maxAudienceCount";
        const string D_Encounter = "encounter";
        const string D_Songs = "requiredSongCount";

        const string DefaultFolderFallback = "Assets/Resources/Data/Encounters";

        // ─────────────────────────────────────────────────────────────── entry points
        [MenuItem("ALWTTT/Encounters/Gig Encounter Wizard")]
        public static void Open()
        {
            var w = GetWindow<GigEncounterWizard>("Gig Encounter Wizard");
            w.minSize = new Vector2(LeftMin + SplitterW + RightMin, 420f);
            w.Show();
        }

        /// <summary>Cross-link entry point (mirror of AudienceMemberWizard.OpenAndSelect). No callers yet.</summary>
        public static void OpenAndSelect(GigEncounterSO encounter)
        {
            Open();
            var w = GetWindow<GigEncounterWizard>();
            w.Rescan();
            w.Select(encounter);
        }

        // ─────────────────────────────────────────────────────────────── data model
        sealed class Badge
        {
            public readonly string Text; public readonly bool Info;
            public Badge(string text, bool info) { Text = text; Info = info; }
        }

        sealed class MemberInfo
        {
            public AudienceCharacterData Asset;
            public string Name, Id, TasteSummary;
            public int MaxVibe, AbilityCount, PreferRoles, TsPref, TsDis, TonPref, TonDis;
            public bool Cyclic, HasPrefab, Fast, Slow, Rich;
            public readonly List<string> Badges = new List<string>();
            public readonly List<string> MissingFields = new List<string>();
            public bool Neutral => !Fast && !Slow && !Rich && TsPref + TsDis == 0 && TonPref + TonDis == 0;
        }

        sealed class Row
        {
            public GigEncounterSO Asset;
            public string Path, AssetName, DisplayName, Venue;
            public int Songs, Fans, CohLoss;
            public int Size, NullCount, Distinct, VibeSum;
            public bool HasDuplicates, IsDemo;
            public bool? InRoster;                       // null = roster unknown/ambiguous
            public readonly List<MemberInfo> Members = new List<MemberInfo>(); // index-aligned, null for null refs
            public readonly List<Badge> Badges = new List<Badge>();
            public readonly List<string> MissingFields = new List<string>();
            public int ErrorCount => Badges.Count(b => !b.Info);
        }

        // ─────────────────────────────────────────────────────────────── state
        readonly List<Row> _rows = new List<Row>();
        readonly List<Row> _visible = new List<Row>();
        readonly Dictionary<AudienceCharacterData, MemberInfo> _memberCache = new Dictionary<AudienceCharacterData, MemberInfo>();
        readonly List<AudienceCharacterData> _archetypes = new List<AudienceCharacterData>();
        string[] _archetypeNames = { "All" };

        GigSetupRosterSO _roster; int _rosterCount; int _maxAudience = -1;
        readonly HashSet<GigEncounterSO> _rosterEncounters = new HashSet<GigEncounterSO>();
        DemoLaunchConfigSO _demo; int _demoCount; GigEncounterSO _demoEncounter; int _demoSongs = -1;

        Row _sel; SerializedObject _so;
        Vector2 _leftScroll, _rightScroll;
        float _leftWidth = 560f; bool _dragging;

        // filters
        string _search = "";
        int _rosterFilter;                  // 0 all · 1 in roster · 2 out of roster
        static readonly string[] RosterFilterNames = { "Roster: all", "Roster: in", "Roster: out" };
        int _sizeMin = 0, _sizeMax = 99;
        int _archetypeFilter;               // 0 = All

        // create panel
        bool _showCreate; string _newName = "GigEncounter New"; string _newFolder; int _templateIndex;

        // styles (lazy, inside OnGUI)
        GUIStyle _left, _right, _center, _dim, _dimRight, _dimCenter, _header, _headerRight, _headerCenter, _badgeErr, _badgeInfo, _prose;
        bool _stylesReady;

        // ─────────────────────────────────────────────────────────────── lifecycle
        void OnEnable() { Rescan(); }
        void OnFocus() { Rescan(); }
        void OnProjectChange() { Rescan(); }

        // ─────────────────────────────────────────────────────────────── scanning
        void Rescan()
        {
            var keep = _sel != null ? _sel.Asset : null;

            _memberCache.Clear(); _archetypes.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:AudienceCharacterData"))
            {
                var a = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (a == null) continue;
                _archetypes.Add(a);
                _memberCache[a] = BuildMember(a);
            }
            _archetypes.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
            var names = new List<string> { "All" };
            names.AddRange(_archetypes.Select(a => a.name));
            _archetypeNames = names.ToArray();
            _archetypeFilter = Mathf.Clamp(_archetypeFilter, 0, _archetypeNames.Length - 1);

            // Roster: validation only when exactly one asset exists (D-GEW-5=A)
            _roster = null; _rosterCount = 0; _maxAudience = -1; _rosterEncounters.Clear();
            var rosterGuids = AssetDatabase.FindAssets("t:GigSetupRosterSO");
            _rosterCount = rosterGuids.Length;
            if (_rosterCount == 1)
            {
                _roster = AssetDatabase.LoadAssetAtPath<GigSetupRosterSO>(AssetDatabase.GUIDToAssetPath(rosterGuids[0]));
                if (_roster != null)
                {
                    var rso = new SerializedObject(_roster);
                    var pMax = rso.FindProperty(R_MaxAudience);
                    _maxAudience = pMax != null ? Mathf.Max(1, pMax.intValue) : -1;
                    var pEnc = rso.FindProperty(R_Encounters);
                    if (pEnc != null && pEnc.isArray)
                        for (int i = 0; i < pEnc.arraySize; i++)
                        {
                            var e = pEnc.GetArrayElementAtIndex(i).objectReferenceValue as GigEncounterSO;
                            if (e != null) _rosterEncounters.Add(e);
                        }
                }
            }

            // Demo launch config: "is this the demo gig?" flag (D-GEW-7=A)
            _demo = null; _demoEncounter = null; _demoSongs = -1;
            var demoGuids = AssetDatabase.FindAssets("t:DemoLaunchConfigSO");
            _demoCount = demoGuids.Length;
            if (_demoCount == 1)
            {
                _demo = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(AssetDatabase.GUIDToAssetPath(demoGuids[0]));
                if (_demo != null)
                {
                    var dso = new SerializedObject(_demo);
                    _demoEncounter = dso.FindProperty(D_Encounter)?.objectReferenceValue as GigEncounterSO;
                    var pS = dso.FindProperty(D_Songs);
                    _demoSongs = pS != null ? pS.intValue : -1;
                }
            }

            _rows.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:GigEncounterSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var a = AssetDatabase.LoadAssetAtPath<GigEncounterSO>(path);
                if (a == null) continue;
                _rows.Add(BuildRow(a, path));
            }
            _rows.Sort((x, y) => string.Compare(x.AssetName, y.AssetName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(_newFolder))
                _newFolder = _rows.Count > 0 ? Path.GetDirectoryName(_rows[0].Path)?.Replace('\\', '/') : DefaultFolderFallback;

            ApplyFilters();
            if (keep != null) Select(keep); else { _sel = null; _so = null; }
            Repaint();
        }

        MemberInfo BuildMember(AudienceCharacterData a)
        {
            var m = new MemberInfo { Asset = a };
            var so = new SerializedObject(a);
            foreach (var f in MemberFields) if (so.FindProperty(f) == null) m.MissingFields.Add(f);

            m.Id = so.FindProperty(M_Id)?.stringValue ?? "";
            m.Name = so.FindProperty(M_Name)?.stringValue;
            if (string.IsNullOrWhiteSpace(m.Name)) m.Name = a.name;
            m.MaxVibe = so.FindProperty(M_Vibe)?.intValue ?? 0;
            m.HasPrefab = so.FindProperty(M_Prefab)?.objectReferenceValue != null;
            var ab = so.FindProperty(M_Abilities);
            m.AbilityCount = (ab != null && ab.isArray) ? ab.arraySize : 0;
            m.Cyclic = so.FindProperty(M_Pattern)?.boolValue ?? false;

            var t = so.FindProperty(M_Taste);
            if (t != null)
            {
                m.Fast = t.FindPropertyRelative("tempoMatchOnFast")?.boolValue ?? false;
                m.Slow = t.FindPropertyRelative("tempoMismatchOnSlow")?.boolValue ?? false;
                m.Rich = t.FindPropertyRelative("roleCountMatchOnRich")?.boolValue ?? false;
                m.PreferRoles = t.FindPropertyRelative("preferAtLeastRoles")?.intValue ?? 0;
                float above = t.FindPropertyRelative("preferAboveTempoScale")?.floatValue ?? 1f;
                float below = t.FindPropertyRelative("dislikeBelowTempoScale")?.floatValue ?? 1f;
                m.TsPref = ArraySize(t.FindPropertyRelative("preferredTimeSignatures"));
                m.TsDis = ArraySize(t.FindPropertyRelative("dislikedTimeSignatures"));
                m.TonPref = ArraySize(t.FindPropertyRelative("preferredTonalities"));
                m.TonDis = ArraySize(t.FindPropertyRelative("dislikedTonalities"));

                var parts = new List<string>();
                if (m.Fast) parts.Add($"tempo↑>{above:0.##}");
                if (m.Slow) parts.Add($"tempo↓<{below:0.##}");
                if (m.Rich) parts.Add($"roles≥{m.PreferRoles}");
                if (m.TsPref + m.TsDis > 0) parts.Add($"TS +{m.TsPref}/−{m.TsDis}");
                if (m.TonPref + m.TonDis > 0) parts.Add($"ton +{m.TonPref}/−{m.TonDis}");
                m.TasteSummary = parts.Count > 0 ? string.Join(" · ", parts) : "neutral";
            }
            else m.TasteSummary = "?";

            // AMW-1 error badges only (informational NEUTRAL TASTE is not an error → not a "member with badges")
            if (m.AbilityCount == 0) m.Badges.Add("SIN HABILIDADES");
            if (!m.HasPrefab) m.Badges.Add("SIN PREFAB");
            if (m.MaxVibe <= 0) m.Badges.Add("maxVibe ≤ 0");
            return m;
        }

        static int ArraySize(SerializedProperty p) => (p != null && p.isArray) ? p.arraySize : 0;

        Row BuildRow(GigEncounterSO a, string path)
        {
            var r = new Row { Asset = a, Path = path, AssetName = a.name };
            var so = new SerializedObject(a);
            foreach (var f in EncounterFields) if (so.FindProperty(f) == null) r.MissingFields.Add(f);

            r.DisplayName = so.FindProperty(F_DisplayName)?.stringValue ?? "";
            var pv = so.FindProperty(F_Venue);
            r.Venue = (pv != null && pv.enumDisplayNames != null && pv.enumValueIndex >= 0 && pv.enumValueIndex < pv.enumDisplayNames.Length)
                ? pv.enumDisplayNames[pv.enumValueIndex] : "?";
            r.Songs = so.FindProperty(F_Songs)?.intValue ?? 0;
            r.Fans = so.FindProperty(F_Fans)?.intValue ?? 0;
            r.CohLoss = so.FindProperty(F_CohLoss)?.intValue ?? 0;

            var list = so.FindProperty(F_Audience);
            var distinct = new HashSet<AudienceCharacterData>();
            if (list != null && list.isArray)
            {
                r.Size = list.arraySize;
                for (int i = 0; i < list.arraySize; i++)
                {
                    var m = list.GetArrayElementAtIndex(i).objectReferenceValue as AudienceCharacterData;
                    if (m == null) { r.NullCount++; r.Members.Add(null); continue; }
                    if (!_memberCache.TryGetValue(m, out var info)) { info = BuildMember(m); _memberCache[m] = info; }
                    r.Members.Add(info);
                    distinct.Add(m);
                    r.VibeSum += info.MaxVibe;
                }
            }
            r.Distinct = distinct.Count;
            int nonNull = r.Size - r.NullCount;
            r.HasDuplicates = nonNull > r.Distinct;
            r.InRoster = _rosterCount == 1 ? (bool?)_rosterEncounters.Contains(a) : null;
            r.IsDemo = _demoEncounter == a;

            // Badges (requirements §3.7). Info badges never count as errors.
            if (nonNull == 0) r.Badges.Add(new Badge("SIN PÚBLICO", false));
            if (r.NullCount > 0) r.Badges.Add(new Badge($"MIEMBRO NULO ×{r.NullCount}", false));
            if (_maxAudience > 0 && nonNull > _maxAudience) r.Badges.Add(new Badge($"PÚBLICO > MaxAudienceCount ({nonNull} > {_maxAudience})", false));
            if (r.Members.Any(m => m != null && m.Badges.Count > 0)) r.Badges.Add(new Badge("MIEMBRO CON BADGES", false));
            if (r.Songs < 1) r.Badges.Add(new Badge("CANCIONES < 1", false));
            if (r.InRoster == false) r.Badges.Add(new Badge("FUERA DEL ROSTER", true));
            if (r.HasDuplicates) r.Badges.Add(new Badge("DUPLICADOS HORNEADOS", true));
            return r;
        }

        void RefreshRow(Row r)
        {
            if (r == null) return;
            int idx = _rows.IndexOf(r);
            var nr = BuildRow(r.Asset, r.Path);
            if (idx >= 0) _rows[idx] = nr;
            if (_sel == r) _sel = nr;
            ApplyFilters();
        }

        void ApplyFilters()
        {
            _visible.Clear();
            string q = (_search ?? "").Trim();
            var arch = _archetypeFilter > 0 && _archetypeFilter - 1 < _archetypes.Count ? _archetypes[_archetypeFilter - 1] : null;
            foreach (var r in _rows)
            {
                if (q.Length > 0 && r.AssetName.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0
                                 && (r.DisplayName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0
                                 && r.Venue.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (_rosterFilter == 1 && r.InRoster != true) continue;
                if (_rosterFilter == 2 && r.InRoster != false) continue;
                int nonNull = r.Size - r.NullCount;
                if (nonNull < _sizeMin || nonNull > _sizeMax) continue;
                if (arch != null && !r.Members.Any(m => m != null && m.Asset == arch)) continue;
                _visible.Add(r);
            }
        }

        void Select(GigEncounterSO a)
        {
            _sel = _rows.FirstOrDefault(r => r.Asset == a);
            _so = _sel != null ? new SerializedObject(_sel.Asset) : null;
            _rightScroll = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────────────── column grid (rules 1, 11)
        struct Col
        {
            public float Fixed; public float Flex; public float Min;
            public static Col F(float w) => new Col { Fixed = w };
            public static Col X(float weight, float min) => new Col { Flex = weight, Min = min };
        }

        static Rect[] BuildCols(Rect r, Col[] cols)
        {
            float fixedSum = 0f, flexSum = 0f;
            foreach (var c in cols) { if (c.Flex > 0f) flexSum += c.Flex; else fixedSum += c.Fixed; }
            float avail = Mathf.Max(0f, r.width - fixedSum - Pad * (cols.Length - 1));
            var rects = new Rect[cols.Length];
            float x = r.x;
            for (int i = 0; i < cols.Length; i++)
            {
                float w = cols[i].Flex > 0f ? Mathf.Max(cols[i].Min, avail * cols[i].Flex / Mathf.Max(0.0001f, flexSum)) : cols[i].Fixed;
                rects[i] = new Rect(x, r.y, w, r.height);
                x += w + Pad;
            }
            return rects;
        }

        // Inventory grid: Name · Venue · Songs · Size · Arch · Vibe · Roster · Demo · !   (fixed sum 376)
        static readonly Col[] InvCols = { Col.X(1f, 90f), Col.F(80f), Col.F(44f), Col.F(40f), Col.F(40f), Col.F(50f), Col.F(48f), Col.F(44f), Col.F(30f) };
        // Member grid: # · Asset · Vibe · Abil · Pat · Taste · ! · ↑ · ↓ · ⧉ · × · →   (fixed sum 272)
        static readonly Col[] MemCols = { Col.F(22f), Col.X(1f, 100f), Col.F(40f), Col.F(32f), Col.F(48f), Col.X(1f, 80f), Col.F(24f), Col.F(20f), Col.F(20f), Col.F(20f), Col.F(20f), Col.F(26f) };

        // ─────────────────────────────────────────────────────────────── GUI
        void EnsureStyles()
        {
            if (_stylesReady) return;
            _left = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            _right = new GUIStyle(_left) { alignment = TextAnchor.MiddleRight };
            _center = new GUIStyle(_left) { alignment = TextAnchor.MiddleCenter };
            var dimColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.38f) : new Color(0f, 0f, 0f, 0.38f);
            _dim = new GUIStyle(_left); _dim.normal.textColor = dimColor;
            _dimRight = new GUIStyle(_right); _dimRight.normal.textColor = dimColor;
            _dimCenter = new GUIStyle(_center); _dimCenter.normal.textColor = dimColor;
            _header = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            _headerRight = new GUIStyle(_header) { alignment = TextAnchor.MiddleRight };
            _headerCenter = new GUIStyle(_header) { alignment = TextAnchor.MiddleCenter };
            _badgeErr = new GUIStyle(EditorStyles.miniBoldLabel) { wordWrap = true };
            _badgeErr.normal.textColor = new Color(1f, 0.62f, 0.25f);
            _badgeInfo = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true }; _badgeInfo.normal.textColor = dimColor;
            _prose = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = false };
            _stylesReady = true;
        }

        void OnGUI()
        {
            EnsureStyles();
            DrawToolbar();
            DrawBanners();
            if (_showCreate) DrawCreatePanel();

            using (new EditorGUILayout.HorizontalScope())
            {
                _leftWidth = Mathf.Clamp(_leftWidth, LeftMin, Mathf.Max(LeftMin, position.width - RightMin - SplitterW));
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(_leftWidth))) DrawInventory();
                DrawSplitter();
                using (new EditorGUILayout.VerticalScope()) DrawEditor();
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Search:", GUILayout.Width(50f));
                EditorGUI.BeginChangeCheck();
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.Width(180f));
                _rosterFilter = EditorGUILayout.Popup(_rosterFilter, RosterFilterNames, EditorStyles.toolbarPopup, GUILayout.Width(92f));
                GUILayout.Label("Size", GUILayout.Width(30f));
                _sizeMin = Mathf.Clamp(EditorGUILayout.IntField(_sizeMin, EditorStyles.toolbarTextField, GUILayout.Width(30f)), 0, 99);
                GUILayout.Label("–", GUILayout.Width(10f));
                _sizeMax = Mathf.Clamp(EditorGUILayout.IntField(_sizeMax, EditorStyles.toolbarTextField, GUILayout.Width(30f)), _sizeMin, 99);
                _archetypeFilter = EditorGUILayout.Popup(_archetypeFilter, _archetypeNames, EditorStyles.toolbarPopup, GUILayout.Width(120f));
                if (EditorGUI.EndChangeCheck()) ApplyFilters();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_showCreate ? "New ▾" : "New...", EditorStyles.toolbarButton, GUILayout.Width(56f))) _showCreate = !_showCreate;
                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(84f))) ExportJson();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f))) Rescan();
            }

            // Rule 7: one label, one tooltip.
            string rosterTxt = _rosterCount == 1 && _roster != null ? $"{_roster.name} (max {_maxAudience})" : $"{_rosterCount} assets";
            string demoTxt = _demoCount == 1 && _demo != null ? _demo.name : $"{_demoCount} assets";
            var counter = new GUIContent(
                $"GigEncounterSO assets: {_rows.Count} · shown: {_visible.Count} · roster: {rosterTxt} · demo: {demoTxt}",
                "Roster = the single GigSetupRosterSO used for MaxAudienceCount and AvailableEncounters. " +
                "Demo = the single DemoLaunchConfigSO whose 'encounter' marks the demo gig. " +
                "If either count is not exactly 1, the dependent validation is disabled and a banner says so.");
            EditorGUILayout.LabelField(counter, EditorStyles.miniLabel);
        }

        void DrawBanners()
        {
            var missing = _rows.Where(r => r.MissingFields.Count > 0).ToList();
            if (missing.Count > 0)
                EditorGUILayout.HelpBox("GigEncounterSO serialized field(s) not found — derived columns show zeros: " +
                    string.Join(", ", missing.SelectMany(r => r.MissingFields).Distinct()) +
                    ". The asset shape changed; update the field names in GigEncounterWizard.", MessageType.Error);
            var mm = _memberCache.Values.Where(m => m.MissingFields.Count > 0).ToList();
            if (mm.Count > 0)
                EditorGUILayout.HelpBox("AudienceCharacterData serialized field(s) not found: " +
                    string.Join(", ", mm.SelectMany(m => m.MissingFields).Distinct()) + ".", MessageType.Error);
            if (_rosterCount != 1)
                EditorGUILayout.HelpBox($"GigSetupRosterSO assets found: {_rosterCount}. Roster validation (FUERA DEL ROSTER, PÚBLICO > MaxAudienceCount) is disabled until exactly one exists.", MessageType.Warning);
            if (_demoCount != 1)
                EditorGUILayout.HelpBox($"DemoLaunchConfigSO assets found: {_demoCount}. The Demo column is disabled until exactly one exists.", MessageType.Warning);
        }

        // ─────────────────────────────────────────────────────────────── create panel
        void DrawCreatePanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("New encounter", EditorStyles.boldLabel);
                _newName = EditorGUILayout.TextField("Asset name", _newName);
                _newFolder = EditorGUILayout.TextField("Folder", _newFolder);
                var tmpl = new string[_rows.Count + 1]; tmpl[0] = "(none — empty encounter)";
                for (int i = 0; i < _rows.Count; i++) tmpl[i + 1] = _rows[i].AssetName;
                _templateIndex = Mathf.Clamp(EditorGUILayout.Popup("Template", _templateIndex, tmpl), 0, tmpl.Length - 1);
                EditorGUILayout.LabelField("displayName is set to the asset name (same rule as AudienceMemberWizard §21.8). Asset creation is not undoable.", EditorStyles.miniLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create", GUILayout.Width(90f)))
                    {
                        var template = _templateIndex > 0 && _templateIndex - 1 < _rows.Count ? _rows[_templateIndex - 1].Asset : null;
                        CreateEncounter(_newName, _newFolder, template);
                        _showCreate = false;
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(70f))) _showCreate = false;
                }
            }
        }

        void CreateEncounter(string assetName, string folder, GigEncounterSO template)
        {
            assetName = (assetName ?? "").Trim();
            if (assetName.Length == 0) { EditorUtility.DisplayDialog("Gig Encounter Wizard", "Asset name is empty.", "OK"); return; }
            folder = (folder ?? "").Trim().TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(folder)) { EditorUtility.DisplayDialog("Gig Encounter Wizard", $"Folder does not exist:\n{folder}", "OK"); return; }

            var asset = template != null ? Instantiate(template) : CreateInstance<GigEncounterSO>();
            var so = new SerializedObject(asset);
            var pName = so.FindProperty(F_DisplayName);
            if (pName != null) pName.stringValue = assetName;
            so.ApplyModifiedPropertiesWithoutUndo();

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Rescan();
            Select(asset);
            EditorGUIUtility.PingObject(asset);
        }

        // ─────────────────────────────────────────────────────────────── inventory (left)
        void DrawInventory()
        {
            // Header outside the scroll, same grid, 1 px rule (rule 6)
            var hr = GUILayoutUtility.GetRect(0, HeaderH, GUILayout.ExpandWidth(true));
            var hc = BuildCols(hr, InvCols);
            GUI.Label(hc[0], "Name", _header);
            GUI.Label(hc[1], "Venue", _header);
            GUI.Label(hc[2], "Songs", _headerRight);
            GUI.Label(hc[3], "Size", _headerRight);
            GUI.Label(hc[4], new GUIContent("Arch", "Distinct archetypes (size ≠ variety: [Kid, Kid, Kid] is 3 / 1)"), _headerRight);
            GUI.Label(hc[5], new GUIContent("Vibe", "Sum of maxVibe over the baked audience (duplicates counted)"), _headerRight);
            GUI.Label(hc[6], new GUIContent("Roster", "Listed in GigSetupRosterSO.AvailableEncounters (selectable in the picker)"), _headerCenter);
            GUI.Label(hc[7], new GUIContent("Demo", "Referenced by DemoLaunchConfigSO.encounter (the auto-launch gig)"), _headerCenter);
            GUI.Label(hc[8], new GUIContent("!", "Error-badge count (see panel)"), _headerCenter);
            EditorGUI.DrawRect(new Rect(hr.x, hr.yMax, hr.width, 1f), new Color(1f, 1f, 1f, 0.18f));

            using (var sv = new EditorGUILayout.ScrollViewScope(_leftScroll))
            {
                _leftScroll = sv.scrollPosition;
                var e = Event.current;
                for (int i = 0; i < _visible.Count; i++)
                {
                    var r = _visible[i];
                    var rr = GUILayoutUtility.GetRect(0, RowH, GUILayout.ExpandWidth(true));
                    bool selected = _sel == r;
                    if (selected) EditorGUI.DrawRect(rr, new Color(0.24f, 0.48f, 0.90f, 0.35f));
                    else if ((i & 1) == 1) EditorGUI.DrawRect(rr, new Color(1f, 1f, 1f, 0.03f));

                    if (e.type == EventType.MouseDown && rr.Contains(e.mousePosition))
                    {
                        Select(r.Asset);
                        if (e.clickCount == 2) EditorGUIUtility.PingObject(r.Asset);
                        e.Use(); Repaint();
                    }

                    var c = BuildCols(rr, InvCols);
                    GUI.Label(c[0], new GUIContent(r.AssetName, string.IsNullOrEmpty(r.DisplayName) ? "displayName empty → GetLabel() fallback" : $"displayName: {r.DisplayName}"), _left);
                    GUI.Label(c[1], r.Venue, _left);
                    GUI.Label(c[2], r.Songs.ToString(), r.Songs < 1 ? _dimRight : _right);
                    int nonNull = r.Size - r.NullCount;
                    GUI.Label(c[3], nonNull.ToString(), nonNull == 0 ? _dimRight : _right);
                    GUI.Label(c[4], r.Distinct.ToString(), r.Distinct == 0 ? _dimRight : _right);
                    GUI.Label(c[5], r.VibeSum.ToString(), r.VibeSum == 0 ? _dimRight : _right);
                    GUI.Label(c[6], r.InRoster == null ? "?" : (r.InRoster == true ? "✓" : "·"), r.InRoster == true ? _center : _dimCenter);
                    GUI.Label(c[7], _demoCount != 1 ? "?" : (r.IsDemo ? "✓" : "·"), r.IsDemo ? _center : _dimCenter);
                    int errs = r.ErrorCount;
                    GUI.Label(c[8], new GUIContent(errs > 0 ? errs.ToString() : "·", string.Join("\n", r.Badges.Select(b => b.Text))), errs > 0 ? _center : _dimCenter);
                }
                if (_visible.Count == 0) EditorGUILayout.LabelField("No encounters match the current filters.", _dim);
            }
        }

        void DrawSplitter()
        {
            var sr = GUILayoutUtility.GetRect(SplitterW, SplitterW, GUILayout.ExpandHeight(true), GUILayout.Width(SplitterW));
            EditorGUI.DrawRect(sr, new Color(0f, 0f, 0f, 0.35f));
            EditorGUIUtility.AddCursorRect(sr, MouseCursor.ResizeHorizontal);
            var e = Event.current;
            if (e.type == EventType.MouseDown && sr.Contains(e.mousePosition)) { _dragging = true; e.Use(); }
            if (_dragging && e.type == EventType.MouseDrag) { _leftWidth += e.delta.x; e.Use(); Repaint(); }
            if (e.type == EventType.MouseUp) _dragging = false;
        }

        // ─────────────────────────────────────────────────────────────── editor (right)
        void DrawEditor()
        {
            if (_sel == null || _so == null)
            {
                EditorGUILayout.LabelField("Select an encounter on the left, or create one with New...", _dim);
                return;
            }
            if (_sel.Asset == null) { _sel = null; _so = null; return; }
            _so.Update();

            using (var sv = new EditorGUILayout.ScrollViewScope(_rightScroll))
            {
                _rightScroll = sv.scrollPosition;

                EditorGUILayout.LabelField(_sel.AssetName, EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true)) EditorGUILayout.TextField("Path", _sel.Path);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Ping")) EditorGUIUtility.PingObject(_sel.Asset);
                    if (GUILayout.Button("Duplicate")) { DuplicateSelected(); GUIUtility.ExitGUI(); }
                    if (GUILayout.Button("Delete")) { DeleteSelected(); GUIUtility.ExitGUI(); }
                }

                // Referenced-by (mirror of AMW panel block)
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Referenced by", EditorStyles.miniBoldLabel);
                bool anyRef = false;
                if (_sel.InRoster == true && _roster != null)
                {
                    anyRef = true;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{_roster.name} (GigSetupRosterSO.AvailableEncounters)");
                        if (GUILayout.Button("Ping", GUILayout.Width(50f))) EditorGUIUtility.PingObject(_roster);
                    }
                }
                if (_sel.IsDemo && _demo != null)
                {
                    anyRef = true;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{_demo.name} (DemoLaunchConfigSO.encounter — the auto-launch gig)");
                        if (GUILayout.Button("Ping", GUILayout.Width(50f))) EditorGUIUtility.PingObject(_demo);
                    }
                }
                if (!anyRef) EditorGUILayout.LabelField(_sel.InRoster == null ? "roster unknown (see banner)" : "none — not selectable in the picker and not the demo gig", _dim);

                // Validation badges
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Validation", EditorStyles.miniBoldLabel);
                if (_sel.Badges.Count == 0) EditorGUILayout.LabelField("no badges", _dim);
                foreach (var b in _sel.Badges) EditorGUILayout.LabelField(BadgeLine(b), b.Info ? _badgeInfo : _badgeErr);

                // Fields (SerializedProperty, D-GEW-EDIT)
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Encounter", EditorStyles.boldLabel);
                PropertyOrMissing(F_DisplayName, "Display Name");
                if (string.IsNullOrWhiteSpace(_so.FindProperty(F_DisplayName)?.stringValue))
                    EditorGUILayout.LabelField("empty → picker/logs show GetLabel(): \"<venue> | Songs:n | FansWin:n | CohLoss:n\"", EditorStyles.miniLabel);
                PropertyOrMissing(F_Venue, "Target Venue Type");

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Gig Requirements", EditorStyles.boldLabel);
                PropertyOrMissing(F_Songs, "Number Of Songs");
                if (_sel.IsDemo && _demoSongs > 0)
                    EditorGUILayout.LabelField($"Demo auto-launch overrides this with DemoLaunchConfig.requiredSongCount = {_demoSongs} (overrideRequiredSongCount = true).", EditorStyles.miniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Rewards / Penalties", EditorStyles.boldLabel);
                PropertyOrMissing(F_Fans, "Fans On Win");
                PropertyOrMissing(F_CohLoss, "Cohesion Penalty On Loss");

                // Audience editor
                EditorGUILayout.Space(8f);
                DrawAudienceEditor();

                // Prose (restatement, never simulation — D-GEW-SIM)
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Composition (restatement of authored fields — not a difficulty estimate)", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(BuildProse(_sel), _prose);
            }

            if (_so.ApplyModifiedProperties()) RefreshRow(_sel);
        }

        static string BadgeLine(Badge b)
        {
            switch (b.Text)
            {
                case "SIN PÚBLICO": return "SIN PÚBLICO — no non-null audience entries; BuildRuntime yields an empty audience.";
                case "FUERA DEL ROSTER": return "FUERA DEL ROSTER — not in GigSetupRosterSO.AvailableEncounters; unreachable from the picker (informational).";
                case "DUPLICADOS HORNEADOS": return "DUPLICADOS HORNEADOS — repeated archetypes are honoured on the default launch path and lost if the player customises the audience (SSoT_Gig_Encounter §7.5; informational).";
                case "MIEMBRO CON BADGES": return "MIEMBRO CON BADGES — an audience archetype carries SIN HABILIDADES / SIN PREFAB / maxVibe ≤ 0 (fix it in the Audience Member Wizard).";
                case "CANCIONES < 1": return "CANCIONES < 1 — numberOfSongs below the [Min(1)] floor; runtime clamps to 1.";
                default:
                    return b.Text.StartsWith("PÚBLICO >") ? b.Text + " — the picker blocks Start above MaxAudienceCount." :
                                b.Text.StartsWith("MIEMBRO NULO") ? b.Text + " — null entries in audienceMemberList." : b.Text;
            }
        }

        void PropertyOrMissing(string field, string label)
        {
            var p = _so.FindProperty(field);
            if (p == null) { EditorGUILayout.HelpBox($"Serialized field '{field}' not found on GigEncounterSO.", MessageType.Error); return; }
            EditorGUILayout.PropertyField(p, new GUIContent(label));
        }

        void DrawAudienceEditor()
        {
            var list = _so.FindProperty(F_Audience);
            if (list == null || !list.isArray) { EditorGUILayout.HelpBox($"Serialized field '{F_Audience}' not found or not a list.", MessageType.Error); return; }

            int nonNull = _sel.Size - _sel.NullCount;
            string cap = _maxAudience > 0 ? $" · max {_maxAudience}" : "";
            EditorGUILayout.LabelField($"Audience ({nonNull} of {list.arraySize} entries{cap} · {_sel.Distinct} archetypes)", EditorStyles.boldLabel);

            var hr = GUILayoutUtility.GetRect(0, HeaderH, GUILayout.ExpandWidth(true));
            var hc = BuildCols(hr, MemCols);
            GUI.Label(hc[0], "#", _headerCenter);
            GUI.Label(hc[1], "Archetype", _header);
            GUI.Label(hc[2], "Vibe", _headerRight);
            GUI.Label(hc[3], "Abil", _headerRight);
            GUI.Label(hc[4], "Pattern", _headerCenter);
            GUI.Label(hc[5], "Taste", _header);
            GUI.Label(hc[6], new GUIContent("!", "AMW-1 error badges on this archetype"), _headerCenter);
            GUI.Label(hc[11], new GUIContent("AMW", "Open in Audience Member Wizard"), _headerCenter);
            EditorGUI.DrawRect(new Rect(hr.x, hr.yMax, hr.width, 1f), new Color(1f, 1f, 1f, 0.18f));

            for (int i = 0; i < list.arraySize; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                var rr = GUILayoutUtility.GetRect(0, RowH, GUILayout.ExpandWidth(true));
                if ((i & 1) == 1) EditorGUI.DrawRect(rr, new Color(1f, 1f, 1f, 0.03f));
                var c = BuildCols(rr, MemCols);
                var info = i < _sel.Members.Count ? _sel.Members[i] : null;

                GUI.Label(c[0], (i + 1).ToString(), _dimCenter);
                EditorGUI.ObjectField(c[1], el, typeof(AudienceCharacterData), GUIContent.none);
                if (info != null)
                {
                    GUI.Label(c[2], info.MaxVibe.ToString(), info.MaxVibe <= 0 ? _dimRight : _right);
                    GUI.Label(c[3], info.AbilityCount.ToString(), info.AbilityCount == 0 ? _dimRight : _right);
                    GUI.Label(c[4], info.Cyclic ? "cyclic" : "random", info.Cyclic ? _center : _dimCenter);
                    GUI.Label(c[5], new GUIContent(info.TasteSummary, info.TasteSummary), info.Neutral ? _dim : _left);
                    GUI.Label(c[6], new GUIContent(info.Badges.Count > 0 ? info.Badges.Count.ToString() : "·", string.Join("\n", info.Badges)), info.Badges.Count > 0 ? _center : _dimCenter);
                }
                else
                {
                    GUI.Label(c[2], "·", _dimRight); GUI.Label(c[3], "·", _dimRight); GUI.Label(c[4], "·", _dimCenter);
                    GUI.Label(c[5], "null entry", _dim); GUI.Label(c[6], "·", _dimCenter);
                }

                // Structural edits: apply → refresh → abort frame (rule 8)
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUI.Button(c[7], "↑")) { list.MoveArrayElement(i, i - 1); Commit(); }
                using (new EditorGUI.DisabledScope(i == list.arraySize - 1))
                    if (GUI.Button(c[8], "↓")) { list.MoveArrayElement(i, i + 1); Commit(); }
                if (GUI.Button(c[9], new GUIContent("⧉", "Duplicate entry"))) { list.InsertArrayElementAtIndex(i); Commit(); }
                if (GUI.Button(c[10], new GUIContent("×", "Remove entry")))
                {
                    string who = info != null ? info.Name : "null entry";
                    if (EditorUtility.DisplayDialog("Remove audience entry", $"Remove entry {i + 1} ({who}) from {_sel.AssetName}?", "Remove", "Cancel"))
                    {
                        if (el.objectReferenceValue != null) el.objectReferenceValue = null; // object-ref arrays: null first, then delete (Unity quirk)
                        list.DeleteArrayElementAtIndex(i);
                        Commit();
                    }
                }
                using (new EditorGUI.DisabledScope(info == null))
                    if (GUI.Button(c[11], new GUIContent("→", "Open in Audience Member Wizard"))) AudienceMemberWizard.OpenAndSelect(info.Asset);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add entry", GUILayout.Width(100f)))
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = null; // rule 9: arraySize++ copies the previous element
                    Commit();
                }
                if (_maxAudience > 0 && nonNull > _maxAudience)
                    EditorGUILayout.LabelField($"{nonNull} > MaxAudienceCount {_maxAudience}: the picker blocks Start; the demo path does not validate.", _badgeErr);
            }
        }

        /// <summary>Apply a structural list change, rebuild derived data and abort the IMGUI frame.</summary>
        void Commit()
        {
            _so.ApplyModifiedProperties();
            RefreshRow(_sel);
            GUIUtility.ExitGUI();
        }

        string BuildProse(Row r)
        {
            var members = r.Members.Where(m => m != null).ToList();
            if (members.Count == 0) return "No audience. Nothing to restate.";

            var counts = members.GroupBy(m => m.Asset).OrderByDescending(g => g.Count()).ThenBy(g => g.Key.name)
                                .Select(g => g.Count() > 1 ? $"{g.Key.name} ×{g.Count()}" : g.Key.name);
            int fast = members.Count(m => m.Fast), slow = members.Count(m => m.Slow), rich = members.Count(m => m.Rich);
            int meter = members.Count(m => m.TsPref + m.TsDis > 0), ton = members.Count(m => m.TonPref + m.TonDis > 0);
            int neutral = members.Count(m => m.Neutral);
            var richRoles = members.Where(m => m.Rich).Select(m => m.PreferRoles).Distinct().OrderBy(x => x).ToList();

            var sb = new StringBuilder();
            sb.Append($"{members.Count} members, {r.Distinct} archetypes ({string.Join(", ", counts)}), {r.VibeSum} Vibe total, {r.Songs} song{(r.Songs == 1 ? "" : "s")}. ");
            var clauses = new List<string>();
            clauses.Add(fast == 0 ? "none reward fast tempo" : $"{fast} reward fast tempo");
            clauses.Add(slow == 0 ? "none punish slow tempo" : $"{slow} punish slow tempo");
            clauses.Add(rich == 0 ? "none reward density" : $"{rich} reward density (≥{string.Join("/", richRoles)} roles)");
            clauses.Add(meter == 0 ? "none react to the time signature" : $"{meter} react to the time signature");
            clauses.Add(ton == 0 ? "none react to tonality" : $"{ton} react to tonality");
            if (neutral > 0) clauses.Add($"{neutral} fully neutral");
            sb.Append(Capitalize(string.Join("; ", clauses))).Append('.');
            if (r.Fans != 0 || r.CohLoss != 0) sb.Append($" Win: +{r.Fans} fans. Loss: −{r.CohLoss} Cohesion.");
            return sb.ToString();
        }

        static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        // ─────────────────────────────────────────────────────────────── asset ops
        void DuplicateSelected()
        {
            if (_sel == null) return;
            string folder = Path.GetDirectoryName(_sel.Path)?.Replace('\\', '/') ?? _newFolder;
            CreateEncounter(_sel.AssetName + " Copy", folder, _sel.Asset);
        }

        void DeleteSelected()
        {
            if (_sel == null) return;
            var refs = new List<string>();
            if (_sel.InRoster == true && _roster != null) refs.Add(_roster.name + " (roster)");
            if (_sel.IsDemo && _demo != null) refs.Add(_demo.name + " (demo launch)");
            if (refs.Count > 0)
            {
                EditorUtility.DisplayDialog("Cannot delete", $"{_sel.AssetName} is referenced by: {string.Join(", ", refs)}.\nRemove the reference first (this window does not edit those assets).", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Delete encounter", $"Delete {_sel.Path}?\nThis is not undoable.", "Delete", "Cancel")) return;
            AssetDatabase.DeleteAsset(_sel.Path);
            AssetDatabase.SaveAssets();
            _sel = null; _so = null;
            Rescan();
        }

        // ─────────────────────────────────────────────────────────────── export (§8.4: informational, not re-importable)
        void ExportJson()
        {
            string path = EditorUtility.SaveFilePanel("Export encounters (filtered view)", "", "GigEncounters.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"  \"exportedAt\": \"{DateTime.Now:yyyy-MM-dd HH:mm}\",\n");
            sb.Append("  \"informationalOnly\": true,\n");
            sb.Append($"  \"filter\": {{ \"search\": {J(_search)}, \"roster\": {J(RosterFilterNames[_rosterFilter])}, \"sizeMin\": {_sizeMin}, \"sizeMax\": {_sizeMax}, \"archetype\": {J(_archetypeNames[_archetypeFilter])} }},\n");
            sb.Append($"  \"roster\": {{ \"asset\": {J(_roster != null ? _roster.name : null)}, \"assetsFound\": {_rosterCount}, \"maxAudienceCount\": {_maxAudience} }},\n");
            sb.Append($"  \"demoLaunchConfig\": {{ \"asset\": {J(_demo != null ? _demo.name : null)}, \"assetsFound\": {_demoCount}, \"encounter\": {J(_demoEncounter != null ? _demoEncounter.name : null)}, \"requiredSongCount\": {_demoSongs} }},\n");
            sb.Append("  \"encounters\": [\n");
            for (int i = 0; i < _visible.Count; i++)
            {
                var r = _visible[i];
                sb.Append("    {\n");
                sb.Append($"      \"assetName\": {J(r.AssetName)},\n      \"assetPath\": {J(r.Path)},\n      \"displayName\": {J(r.DisplayName)},\n");
                sb.Append($"      \"venue\": {J(r.Venue)},\n      \"numberOfSongs\": {r.Songs},\n      \"fansOnWin\": {r.Fans},\n      \"cohesionPenaltyOnLoss\": {r.CohLoss},\n");
                sb.Append($"      \"audienceSize\": {r.Size - r.NullCount},\n      \"nullEntries\": {r.NullCount},\n      \"distinctArchetypes\": {r.Distinct},\n      \"vibeSum\": {r.VibeSum},\n");
                sb.Append($"      \"inRoster\": {(r.InRoster == null ? "null" : r.InRoster.Value ? "true" : "false")},\n      \"isDemo\": {(r.IsDemo ? "true" : "false")},\n");
                sb.Append("      \"audience\": [");
                for (int k = 0; k < r.Members.Count; k++)
                {
                    var m = r.Members[k];
                    if (k > 0) sb.Append(", ");
                    sb.Append(m == null ? "null" : $"{{ \"asset\": {J(m.Asset.name)}, \"characterId\": {J(m.Id)}, \"maxVibe\": {m.MaxVibe}, \"abilities\": {m.AbilityCount}, \"cyclic\": {(m.Cyclic ? "true" : "false")}, \"taste\": {J(m.TasteSummary)}, \"badges\": [{string.Join(", ", m.Badges.Select(J))}] }}");
                }
                sb.Append("],\n");
                sb.Append($"      \"badges\": [{string.Join(", ", r.Badges.Select(b => J(b.Text)))}],\n");
                sb.Append($"      \"composition\": {J(BuildProse(r))}\n");
                sb.Append(i < _visible.Count - 1 ? "    },\n" : "    }\n");
            }
            sb.Append("  ]\n}\n");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Debug.Log($"[GigEncounterWizard] Exported {_visible.Count} encounter(s) to {path}");
        }

        static string J(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder("\"");
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: if (ch < 0x20) sb.Append($"\\u{(int)ch:x4}"); else sb.Append(ch); break;
                }
            }
            return sb.Append('"').ToString();
        }
    }
}
#endif
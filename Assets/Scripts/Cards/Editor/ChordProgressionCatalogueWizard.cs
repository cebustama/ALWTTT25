#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MidiGenPlay;
using MidiGenPlay.Composition;
using static MidiGenPlay.MusicTheory.MusicTheory;
using ChordQuality = MidiGenPlay.MusicTheory.MusicTheory.ChordQuality;
using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

/// <summary>
/// Read-only catalogue browser for chord progression assets and palettes.
/// Scans configured folders, lets designers filter by musical metadata,
/// and selecting a row pings/selects the asset in the Project/Inspector.
/// </summary>
public sealed class ChordProgressionCatalogueWizard : EditorWindow
{
    private const string DefaultFolderA = "Assets/Resources/ScriptableObjects/Chord Progressions";
    private const string DefaultFolderB = "Assets/Resources/Chord Progressions";

    private enum AssetViewMode
    {
        All,
        ProgressionsOnly,
        PalettesOnly
    }

    private enum SortMode
    {
        Name,
        Path,
        Measures,
        TimeSignature,
        EntryCount,
        EventsCount
    }

    private sealed class ProgressionRow
    {
        public ChordProgressionData asset;
        public string path;
        public string displayName;
        public string searchBlob;
        public bool anyTonality;
        public HashSet<Tonality> tonalities;
        public HashSet<ChordQuality> qualities;
        public int eventCount;
    }

    private sealed class PaletteRow
    {
        public ChordProgressionPaletteSO asset;
        public string path;
        public string displayName;
        public string searchBlob;
        public int entryCount;
        public List<ProgressionRow> progressionRows;
        public HashSet<Tonality> tonalities;
        public HashSet<ChordQuality> qualities;
        public HashSet<TimeSignature> timeSignatures;
        public int minMeasures;
        public int maxMeasures;
        public int minSubdivisions;
        public int maxSubdivisions;
        public int maxEventCount;
    }

    [MenuItem("MidiGenPlay/Chord Progression Catalogue Wizard...")]
    public static void Open()
    {
        GetWindow<ChordProgressionCatalogueWizard>("Chord Progression Catalogue");
    }

    [SerializeField] private List<string> scanFolders = new();
    [SerializeField] private string searchText = "";
    [SerializeField] private AssetViewMode assetViewMode = AssetViewMode.All;
    [SerializeField] private SortMode sortMode = SortMode.Name;
    [SerializeField] private bool sortDescending = false;

    [SerializeField] private bool filterByTimeSignature = false;
    [SerializeField] private TimeSignature timeSignatureFilter = TimeSignature.FourFour;

    [SerializeField] private int minMeasures = 0;
    [SerializeField] private int maxMeasures = 0;
    [SerializeField] private int minSubdivisions = 0;
    [SerializeField] private int maxSubdivisions = 0;

    [SerializeField] private bool includeAnyTonalityAssetsWhenTonalityFiltering = true;
    [SerializeField] private bool showProgressionResults = true;
    [SerializeField] private bool showPaletteResults = true;
    [SerializeField] private bool showFilters = true;
    [SerializeField] private bool showFolders = false;

    [SerializeField] private Vector2 mainScroll;
    [SerializeField] private Vector2 progressionScroll;
    [SerializeField] private Vector2 paletteScroll;

    private Dictionary<Tonality, bool> tonalityFlags;
    private Dictionary<ChordQuality, bool> qualityFlags;

    private readonly List<ProgressionRow> allProgressions = new();
    private readonly List<PaletteRow> allPalettes = new();
    private List<ProgressionRow> filteredProgressions = new();
    private List<PaletteRow> filteredPalettes = new();

    private GUIStyle wrapMiniLabel;
    private GUIStyle headerStyle;
    private bool hasScanned;
    private string scanStatus = "Click Refresh to scan the configured folders.";

    private void OnEnable()
    {
        if (scanFolders == null || scanFolders.Count == 0)
        {
            scanFolders = new List<string> { DefaultFolderA, DefaultFolderB };
        }

        EnsureFilterDictionaries();
        EnsureStyles();

        if (!hasScanned)
            RefreshCatalogue();
    }

    private void EnsureFilterDictionaries()
    {
        if (tonalityFlags == null)
        {
            tonalityFlags = Enum.GetValues(typeof(Tonality))
                .Cast<Tonality>()
                .ToDictionary(t => t, _ => false);
        }

        if (qualityFlags == null)
        {
            qualityFlags = Enum.GetValues(typeof(ChordQuality))
                .Cast<ChordQuality>()
                .ToDictionary(q => q, _ => false);
        }
    }

    private void EnsureStyles()
    {
        wrapMiniLabel = new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = true,
            richText = true
        };

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12
        };
    }

    private void OnGUI()
    {
        EnsureFilterDictionaries();
        EnsureStyles();

        mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

        DrawHeader();
        EditorGUILayout.Space(4);
        DrawFoldersSection();
        EditorGUILayout.Space(4);
        DrawFiltersSection();
        EditorGUILayout.Space(8);
        DrawResultsSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Chord Progression Catalogue Wizard", headerStyle);
            EditorGUILayout.LabelField(
                "Read-only browser for ChordProgressionData and ChordProgressionPaletteSO assets.",
                wrapMiniLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                assetViewMode = (AssetViewMode)EditorGUILayout.EnumPopup(
                    new GUIContent("View", "Choose whether to show progressions, palettes, or both."),
                    assetViewMode);

                sortMode = (SortMode)EditorGUILayout.EnumPopup(
                    new GUIContent("Sort", "Sort the visible results by a simple key."),
                    sortMode);

                sortDescending = EditorGUILayout.ToggleLeft("Desc", sortDescending, GUILayout.Width(55));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                {
                    RefreshCatalogue();
                }
            }

            EditorGUILayout.HelpBox(scanStatus, MessageType.Info);
        }
    }

    private void DrawFoldersSection()
    {
        showFolders = EditorGUILayout.BeginFoldoutHeaderGroup(showFolders, "Scan Folders");
        if (!showFolders)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(
                "The wizard scans these folders recursively with AssetDatabase.FindAssets(...). " +
                "Default roots include both the current ScriptableObjects location and the older chord path.",
                wrapMiniLabel);

            if (scanFolders == null)
                scanFolders = new List<string>();

            int removeIndex = -1;
            for (int i = 0; i < scanFolders.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    scanFolders[i] = EditorGUILayout.TextField($"Folder {i + 1}", scanFolders[i]);
                    if (GUILayout.Button("...", GUILayout.Width(30)))
                    {
                        string chosen = EditorUtility.OpenFolderPanel(
                            "Select Unity folder",
                            Application.dataPath,
                            "");

                        if (!string.IsNullOrWhiteSpace(chosen))
                        {
                            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath)?.Replace("\\", "/");
                            chosen = chosen.Replace("\\", "/");
                            if (!string.IsNullOrWhiteSpace(projectRoot) && chosen.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                            {
                                string relative = chosen.Substring(projectRoot.Length + 1);
                                scanFolders[i] = relative;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog(
                                    "Folder Outside Project",
                                    "Please choose a folder inside the current Unity project.",
                                    "OK");
                            }
                        }
                    }

                    if (GUILayout.Button("X", GUILayout.Width(24)))
                        removeIndex = i;
                }
            }

            if (removeIndex >= 0 && removeIndex < scanFolders.Count)
                scanFolders.RemoveAt(removeIndex);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Folder"))
                    scanFolders.Add("Assets/");

                if (GUILayout.Button("Reset Defaults"))
                {
                    scanFolders = new List<string> { DefaultFolderA, DefaultFolderB };
                }
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawFiltersSection()
    {
        showFilters = EditorGUILayout.BeginFoldoutHeaderGroup(showFilters, "Filters");
        if (!showFilters)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUI.BeginChangeCheck();

            searchText = EditorGUILayout.TextField(
                new GUIContent("Search", "Matches asset name, display name, path, original input, and palette notes."),
                searchText);

            using (new EditorGUILayout.HorizontalScope())
            {
                filterByTimeSignature = EditorGUILayout.ToggleLeft("Filter by TS", filterByTimeSignature, GUILayout.Width(95));
                using (new EditorGUI.DisabledScope(!filterByTimeSignature))
                {
                    timeSignatureFilter = (TimeSignature)EditorGUILayout.EnumPopup(timeSignatureFilter);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                minMeasures = EditorGUILayout.IntField(new GUIContent("Min Measures", "0 = ignore."), minMeasures);
                maxMeasures = EditorGUILayout.IntField(new GUIContent("Max Measures", "0 = ignore."), maxMeasures);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                minSubdivisions = EditorGUILayout.IntField(new GUIContent("Min Subdivisions", "0 = ignore."), minSubdivisions);
                maxSubdivisions = EditorGUILayout.IntField(new GUIContent("Max Subdivisions", "0 = ignore."), maxSubdivisions);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Tonality Filter (match ANY selected)", EditorStyles.boldLabel);
            DrawEnumChecklist(tonalityFlags, 3);
            includeAnyTonalityAssetsWhenTonalityFiltering = EditorGUILayout.ToggleLeft(
                "When tonality filtering is active, treat empty tonalities as match-any",
                includeAnyTonalityAssetsWhenTonalityFiltering);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Chord Quality Filter (match ANY selected)", EditorStyles.boldLabel);
            DrawEnumChecklist(qualityFlags, 4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear Filters"))
                {
                    ClearFilters();
                }

                if (GUILayout.Button("Apply Filters"))
                {
                    ApplyFilters();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilters();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawResultsSection()
    {
        int progCount = filteredProgressions?.Count ?? 0;
        int paletteCount = filteredPalettes?.Count ?? 0;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(
                $"Results: {progCount} progression(s), {paletteCount} palette(s)",
                headerStyle);

            if ((assetViewMode == AssetViewMode.All || assetViewMode == AssetViewMode.ProgressionsOnly))
            {
                showProgressionResults = EditorGUILayout.BeginFoldoutHeaderGroup(
                    showProgressionResults,
                    $"ChordProgressionData ({progCount})");

                if (showProgressionResults)
                {
                    progressionScroll = EditorGUILayout.BeginScrollView(progressionScroll, GUILayout.MinHeight(180));
                    if (progCount == 0)
                    {
                        EditorGUILayout.HelpBox("No progression assets matched the current filters.", MessageType.None);
                    }
                    else
                    {
                        foreach (var row in filteredProgressions)
                            DrawProgressionRow(row);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if ((assetViewMode == AssetViewMode.All || assetViewMode == AssetViewMode.PalettesOnly))
            {
                showPaletteResults = EditorGUILayout.BeginFoldoutHeaderGroup(
                    showPaletteResults,
                    $"ChordProgressionPaletteSO ({paletteCount})");

                if (showPaletteResults)
                {
                    paletteScroll = EditorGUILayout.BeginScrollView(paletteScroll, GUILayout.MinHeight(180));
                    if (paletteCount == 0)
                    {
                        EditorGUILayout.HelpBox("No palette assets matched the current filters.", MessageType.None);
                    }
                    else
                    {
                        foreach (var row in filteredPalettes)
                            DrawPaletteRow(row);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }

    private void DrawProgressionRow(ProgressionRow row)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(row.displayName, EditorStyles.linkLabel))
                {
                    SelectAsset(row.asset);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(row.asset.TimeSignature.ToString(), GUILayout.Width(90));
                EditorGUILayout.LabelField($"{row.asset.Measures} bars", GUILayout.Width(55));
                EditorGUILayout.LabelField($"sub x{Mathf.Max(1, row.asset.subdivisions)}", GUILayout.Width(60));
                EditorGUILayout.LabelField($"events {row.eventCount}", GUILayout.Width(60));
            }

            EditorGUILayout.LabelField($"Path: {row.path}", wrapMiniLabel);
            EditorGUILayout.LabelField(
                $"Tonalities: {FormatTonalities(row.asset.tonalities)} | Qualities: {FormatQualities(row.qualities)}",
                wrapMiniLabel);

            if (!string.IsNullOrWhiteSpace(row.asset.originalInput))
                EditorGUILayout.LabelField($"Roman: {row.asset.originalInput}", wrapMiniLabel);
        }
    }

    private void DrawPaletteRow(PaletteRow row)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(row.displayName, EditorStyles.linkLabel))
                {
                    SelectAsset(row.asset);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"entries {row.entryCount}", GUILayout.Width(65));
                EditorGUILayout.LabelField($"TS {FormatTimeSignatureSet(row.timeSignatures)}", GUILayout.Width(150));
            }

            EditorGUILayout.LabelField($"Path: {row.path}", wrapMiniLabel);
            EditorGUILayout.LabelField(
                $"Measures: {FormatRange(row.minMeasures, row.maxMeasures)} | " +
                $"Subdivisions: {FormatRange(row.minSubdivisions, row.maxSubdivisions)} | " +
                $"Tonalities: {FormatTonalities(row.tonalities)}",
                wrapMiniLabel);

            EditorGUILayout.LabelField($"Qualities: {FormatQualities(row.qualities)}", wrapMiniLabel);

            if (!string.IsNullOrWhiteSpace(row.asset.paletteNotes))
                EditorGUILayout.LabelField($"Notes: {row.asset.paletteNotes}", wrapMiniLabel);
        }
    }

    private void DrawEnumChecklist<T>(Dictionary<T, bool> flags, int columns) where T : Enum
    {
        if (flags == null || flags.Count == 0)
            return;

        var values = flags.Keys.ToList();
        int colCount = Mathf.Max(1, columns);
        int rowCount = Mathf.CeilToInt(values.Count / (float)colCount);

        for (int row = 0; row < rowCount; row++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int col = 0; col < colCount; col++)
                {
                    int index = row + col * rowCount;
                    if (index >= values.Count)
                    {
                        GUILayout.FlexibleSpace();
                        continue;
                    }

                    var key = values[index];
                    flags[key] = EditorGUILayout.ToggleLeft(ObjectNames.NicifyVariableName(key.ToString()), flags[key]);
                }
            }
        }
    }

    private void RefreshCatalogue()
    {
        EnsureFilterDictionaries();

        allProgressions.Clear();
        allPalettes.Clear();

        List<string> validFolders = GetValidFolders();

        string[] progressionGuids = validFolders.Count > 0
            ? AssetDatabase.FindAssets("t:ChordProgressionData", validFolders.ToArray())
            : AssetDatabase.FindAssets("t:ChordProgressionData");

        foreach (string guid in progressionGuids.Distinct())
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ChordProgressionData>(path);
            if (asset == null)
                continue;

            allProgressions.Add(BuildProgressionRow(asset, path));
        }

        string[] paletteGuids = validFolders.Count > 0
            ? AssetDatabase.FindAssets("t:ChordProgressionPaletteSO", validFolders.ToArray())
            : AssetDatabase.FindAssets("t:ChordProgressionPaletteSO");

        foreach (string guid in paletteGuids.Distinct())
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ChordProgressionPaletteSO>(path);
            if (asset == null)
                continue;

            allPalettes.Add(BuildPaletteRow(asset, path));
        }

        hasScanned = true;
        scanStatus = $"Scanned {allProgressions.Count} progression(s) and {allPalettes.Count} palette(s).";
        ApplyFilters();
        Repaint();
    }

    private List<string> GetValidFolders()
    {
        var valid = new List<string>();
        if (scanFolders == null)
            return valid;

        foreach (var raw in scanFolders)
        {
            string folder = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(folder))
                continue;

            if (AssetDatabase.IsValidFolder(folder))
                valid.Add(folder);
        }

        return valid.Distinct().ToList();
    }

    private ProgressionRow BuildProgressionRow(ChordProgressionData asset, string path)
    {
        var tonalities = new HashSet<Tonality>();
        if (asset.tonalities != null)
        {
            foreach (var t in asset.tonalities)
                tonalities.Add(t);
        }

        var qualities = new HashSet<ChordQuality>();
        if (asset.events != null)
        {
            foreach (var e in asset.events)
            {
                if (e == null) continue;
                qualities.Add(e.quality);
            }
        }

        string display = !string.IsNullOrWhiteSpace(asset.DisplayName)
            ? asset.DisplayName
            : (!string.IsNullOrWhiteSpace(asset.originalInput) ? asset.originalInput : asset.name);

        string blob = string.Join(" | ", new[]
        {
            asset.name,
            display,
            asset.originalInput,
            path,
            string.Join(" ", tonalities.Select(t => t.ToString())),
            string.Join(" ", qualities.Select(q => q.ToString()))
        }.Where(s => !string.IsNullOrWhiteSpace(s))).ToLowerInvariant();

        return new ProgressionRow
        {
            asset = asset,
            path = path,
            displayName = display,
            searchBlob = blob,
            anyTonality = tonalities.Count == 0,
            tonalities = tonalities,
            qualities = qualities,
            eventCount = asset.events != null ? asset.events.Count : 0
        };
    }

    private PaletteRow BuildPaletteRow(ChordProgressionPaletteSO asset, string path)
    {
        var progressionRows = new List<ProgressionRow>();
        if (asset.entries != null)
        {
            foreach (var entry in asset.entries)
            {
                if (entry == null || entry.progression == null)
                    continue;

                string progPath = AssetDatabase.GetAssetPath(entry.progression);
                progressionRows.Add(BuildProgressionRow(entry.progression, progPath));
            }
        }

        var tonalities = new HashSet<Tonality>();
        var qualities = new HashSet<ChordQuality>();
        var timeSignatures = new HashSet<TimeSignature>();

        int minMeasuresLocal = int.MaxValue;
        int maxMeasuresLocal = 0;
        int minSubLocal = int.MaxValue;
        int maxSubLocal = 0;
        int maxEventCountLocal = 0;

        foreach (var row in progressionRows)
        {
            timeSignatures.Add(row.asset.TimeSignature);
            minMeasuresLocal = Mathf.Min(minMeasuresLocal, row.asset.Measures);
            maxMeasuresLocal = Mathf.Max(maxMeasuresLocal, row.asset.Measures);
            minSubLocal = Mathf.Min(minSubLocal, Mathf.Max(1, row.asset.subdivisions));
            maxSubLocal = Mathf.Max(maxSubLocal, Mathf.Max(1, row.asset.subdivisions));
            maxEventCountLocal = Mathf.Max(maxEventCountLocal, row.eventCount);

            foreach (var t in row.tonalities)
                tonalities.Add(t);
            foreach (var q in row.qualities)
                qualities.Add(q);
        }

        if (minMeasuresLocal == int.MaxValue) minMeasuresLocal = 0;
        if (minSubLocal == int.MaxValue) minSubLocal = 0;

        string display = !string.IsNullOrWhiteSpace(asset.GetDisplayName())
            ? asset.GetDisplayName()
            : asset.name;

        string blob = string.Join(" | ", new[]
        {
            asset.name,
            display,
            asset.paletteNotes,
            path,
            string.Join(" ", progressionRows.Select(p => p.displayName)),
            string.Join(" ", tonalities.Select(t => t.ToString())),
            string.Join(" ", qualities.Select(q => q.ToString()))
        }.Where(s => !string.IsNullOrWhiteSpace(s))).ToLowerInvariant();

        return new PaletteRow
        {
            asset = asset,
            path = path,
            displayName = display,
            searchBlob = blob,
            entryCount = asset.entries != null ? asset.entries.Count(e => e != null && e.progression != null) : 0,
            progressionRows = progressionRows,
            tonalities = tonalities,
            qualities = qualities,
            timeSignatures = timeSignatures,
            minMeasures = minMeasuresLocal,
            maxMeasures = maxMeasuresLocal,
            minSubdivisions = minSubLocal,
            maxSubdivisions = maxSubLocal,
            maxEventCount = maxEventCountLocal
        };
    }

    private void ApplyFilters()
    {
        EnsureFilterDictionaries();

        IEnumerable<ProgressionRow> progQuery = allProgressions;
        progQuery = progQuery.Where(MatchesProgressionFilters);
        progQuery = SortProgressions(progQuery);
        filteredProgressions = progQuery.ToList();

        IEnumerable<PaletteRow> paletteQuery = allPalettes;
        paletteQuery = paletteQuery.Where(MatchesPaletteFilters);
        paletteQuery = SortPalettes(paletteQuery);
        filteredPalettes = paletteQuery.ToList();
    }

    private IEnumerable<ProgressionRow> SortProgressions(IEnumerable<ProgressionRow> query)
    {
        Func<ProgressionRow, object> key = sortMode switch
        {
            SortMode.Path => row => row.path,
            SortMode.Measures => row => row.asset.Measures,
            SortMode.TimeSignature => row => row.asset.TimeSignature,
            SortMode.EventsCount => row => row.eventCount,
            _ => row => row.displayName
        };

        return sortDescending ? query.OrderByDescending(key) : query.OrderBy(key);
    }

    private IEnumerable<PaletteRow> SortPalettes(IEnumerable<PaletteRow> query)
    {
        Func<PaletteRow, object> key = sortMode switch
        {
            SortMode.Path => row => row.path,
            SortMode.Measures => row => row.maxMeasures,
            SortMode.TimeSignature => row => row.timeSignatures.Count > 0 ? row.timeSignatures.OrderBy(ts => ts.ToString()).First() : 0,
            SortMode.EntryCount => row => row.entryCount,
            SortMode.EventsCount => row => row.maxEventCount,
            _ => row => row.displayName
        };

        return sortDescending ? query.OrderByDescending(key) : query.OrderBy(key);
    }

    private bool MatchesProgressionFilters(ProgressionRow row)
    {
        if (row == null || row.asset == null)
            return false;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string s = searchText.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(row.searchBlob) || !row.searchBlob.Contains(s))
                return false;
        }

        if (filterByTimeSignature && row.asset.TimeSignature != timeSignatureFilter)
            return false;

        if (minMeasures > 0 && row.asset.Measures < minMeasures)
            return false;

        if (maxMeasures > 0 && row.asset.Measures > maxMeasures)
            return false;

        int sub = Mathf.Max(1, row.asset.subdivisions);
        if (minSubdivisions > 0 && sub < minSubdivisions)
            return false;
        if (maxSubdivisions > 0 && sub > maxSubdivisions)
            return false;

        var selectedTonalities = GetSelectedTonalities();
        if (selectedTonalities.Count > 0)
        {
            bool tonalityMatch = row.anyTonality
                ? includeAnyTonalityAssetsWhenTonalityFiltering
                : row.tonalities.Overlaps(selectedTonalities);

            if (!tonalityMatch)
                return false;
        }

        var selectedQualities = GetSelectedQualities();
        if (selectedQualities.Count > 0 && !row.qualities.Overlaps(selectedQualities))
            return false;

        return true;
    }

    private bool MatchesPaletteFilters(PaletteRow row)
    {
        if (row == null || row.asset == null)
            return false;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string s = searchText.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(row.searchBlob) || !row.searchBlob.Contains(s))
                return false;
        }

        if (row.progressionRows == null || row.progressionRows.Count == 0)
        {
            return !filterByTimeSignature
                && minMeasures <= 0
                && maxMeasures <= 0
                && minSubdivisions <= 0
                && maxSubdivisions <= 0
                && GetSelectedTonalities().Count == 0
                && GetSelectedQualities().Count == 0;
        }

        return row.progressionRows.Any(MatchesProgressionFilters);
    }

    private HashSet<Tonality> GetSelectedTonalities()
    {
        return tonalityFlags
            .Where(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToHashSet();
    }

    private HashSet<ChordQuality> GetSelectedQualities()
    {
        return qualityFlags
            .Where(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToHashSet();
    }

    private void ClearFilters()
    {
        searchText = string.Empty;
        filterByTimeSignature = false;
        minMeasures = 0;
        maxMeasures = 0;
        minSubdivisions = 0;
        maxSubdivisions = 0;
        includeAnyTonalityAssetsWhenTonalityFiltering = true;

        foreach (var key in tonalityFlags.Keys.ToList())
            tonalityFlags[key] = false;

        foreach (var key in qualityFlags.Keys.ToList())
            qualityFlags[key] = false;

        ApplyFilters();
    }

    private static void SelectAsset(UnityEngine.Object asset)
    {
        if (asset == null)
            return;

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    private static string FormatTonalities(IEnumerable<Tonality> tonalities)
    {
        if (tonalities == null)
            return "Any";

        var list = tonalities.Select(t => t.ToString()).Distinct().ToList();
        return list.Count == 0 ? "Any" : string.Join(", ", list);
    }

    private static string FormatQualities(IEnumerable<ChordQuality> qualities)
    {
        if (qualities == null)
            return "-";

        var list = qualities.Select(q => q.ToString()).Distinct().ToList();
        return list.Count == 0 ? "-" : string.Join(", ", list);
    }

    private static string FormatTimeSignatureSet(IEnumerable<TimeSignature> set)
    {
        if (set == null)
            return "-";

        var list = set.Select(ts => ts.ToString()).Distinct().ToList();
        return list.Count == 0 ? "-" : string.Join(", ", list);
    }

    private static string FormatRange(int min, int max)
    {
        if (min <= 0 && max <= 0)
            return "-";
        if (min == max)
            return min.ToString();
        return $"{min}–{max}";
    }
}
#endif

#if UNITY_EDITOR
using ALWTTT.Cards;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Inventory viewer for ALWTTT card-related assets. Read-only browser with Print + Export JSON
    /// per view. Four views: all CardDefinitions, all MusicianCardCatalogData (with summaries),
    /// one specific musician catalogue, all GenericCardCatalogSO.
    /// Editor-only, batch (3.B).
    /// </summary>
    public sealed class CardInventoryWindow : EditorWindow
    {
        private enum View
        {
            AllCardDefinitions,
            AllMusicianCatalogs,
            SingleMusicianCatalog,
            AllGenericCatalogs
        }

        [SerializeField] private View _view = View.AllCardDefinitions;
        [SerializeField] private MusicianCharacterType _selectedMusician = MusicianCharacterType.None;
        [SerializeField] private Vector2 _scroll;

        [MenuItem("ALWTTT/Cards/Card Inventory", priority = 12)]
        public static void Open()
        {
            var w = GetWindow<CardInventoryWindow>();
            w.titleContent = new GUIContent("Card Inventory");
            w.minSize = new Vector2(640, 420);
            w.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(4);
            using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = s.scrollPosition;
                switch (_view)
                {
                    case View.AllCardDefinitions: DrawAllCardDefinitions(); break;
                    case View.AllMusicianCatalogs: DrawAllMusicianCatalogs(); break;
                    case View.SingleMusicianCatalog: DrawSingleMusicianCatalog(); break;
                    case View.AllGenericCatalogs: DrawAllGenericCatalogs(); break;
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Toolbar
        // ──────────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Toggle(_view == View.AllCardDefinitions,
                        "All CardDefinitions", EditorStyles.toolbarButton, GUILayout.Width(150)))
                    _view = View.AllCardDefinitions;
                if (GUILayout.Toggle(_view == View.AllMusicianCatalogs,
                        "All Musician Catalogs", EditorStyles.toolbarButton, GUILayout.Width(160)))
                    _view = View.AllMusicianCatalogs;
                if (GUILayout.Toggle(_view == View.SingleMusicianCatalog,
                        "One Musician", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    _view = View.SingleMusicianCatalog;
                if (GUILayout.Toggle(_view == View.AllGenericCatalogs,
                        "All Generic Catalogs", EditorStyles.toolbarButton, GUILayout.Width(150)))
                    _view = View.AllGenericCatalogs;

                GUILayout.FlexibleSpace();

                if (_view == View.SingleMusicianCatalog)
                {
                    GUILayout.Label("Musician:", GUILayout.Width(60));
                    _selectedMusician = (MusicianCharacterType)EditorGUILayout.EnumPopup(
                        _selectedMusician, EditorStyles.toolbarPopup, GUILayout.Width(140));
                }

                if (GUILayout.Button("Print", EditorStyles.toolbarButton, GUILayout.Width(56)))
                    PrintCurrentView();
                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    ExportCurrentView();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // View 1 — all CardDefinitions
        // ──────────────────────────────────────────────────────────────────
        private void DrawAllCardDefinitions()
        {
            var defs = FindAllAssets<CardDefinition>();
            EditorGUILayout.LabelField($"CardDefinition assets: {defs.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            foreach (var c in defs)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    string kind = c.IsAction ? "A" : (c.IsComposition ? "C" : "?");
                    GUILayout.Label($"[{kind}]", GUILayout.Width(28));
                    GUILayout.Label(c.Id ?? "<no id>", GUILayout.Width(220));
                    GUILayout.Label(c.DisplayName ?? "<no name>", GUILayout.Width(220));
                    GUILayout.Label($"cost={c.InspirationCost}", GUILayout.Width(60));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(48)))
                        EditorGUIUtility.PingObject(c);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // View 2 — all MusicianCardCatalogData (per-asset summary)
        // ──────────────────────────────────────────────────────────────────
        private void DrawAllMusicianCatalogs()
        {
            var cats = FindAllAssets<MusicianCardCatalogData>();
            EditorGUILayout.LabelField($"MusicianCardCatalogData assets: {cats.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            foreach (var cat in cats)
            {
                int total = cat.Entries?.Count ?? 0;
                int starter = 0, starterCopies = 0;
                if (cat.Entries != null)
                {
                    foreach (var e in cat.Entries)
                    {
                        if (e == null) continue;
                        if (e.IsStarter) { starter++; starterCopies += Mathf.Max(1, e.starterCopies); }
                    }
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"{cat.MusicianType}", GUILayout.Width(120));
                    GUILayout.Label(cat.name, GUILayout.Width(220));
                    GUILayout.Label($"entries={total}", GUILayout.Width(80));
                    GUILayout.Label($"starter={starter}", GUILayout.Width(80));
                    GUILayout.Label($"copies={starterCopies}", GUILayout.Width(80));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(48)))
                        EditorGUIUtility.PingObject(cat);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // View 3 — single musician catalogue (full entry list)
        // ──────────────────────────────────────────────────────────────────
        private void DrawSingleMusicianCatalog()
        {
            if (_selectedMusician == MusicianCharacterType.None)
            {
                EditorGUILayout.HelpBox("Select a musician in the toolbar.", MessageType.Info);
                return;
            }

            var cats = FindAllAssets<MusicianCardCatalogData>();
            MusicianCardCatalogData target = null;
            foreach (var c in cats) if (c.MusicianType == _selectedMusician) { target = c; break; }

            if (target == null)
            {
                EditorGUILayout.HelpBox(
                    $"No MusicianCardCatalogData asset found for {_selectedMusician}.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(
                $"{_selectedMusician} — {target.name} — entries: {target.Entries?.Count ?? 0}",
                EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            DrawEntryList(target.Entries);
        }

        // ──────────────────────────────────────────────────────────────────
        // View 4 — all GenericCardCatalogSO
        // ──────────────────────────────────────────────────────────────────
        private void DrawAllGenericCatalogs()
        {
            var cats = FindAllAssets<GenericCardCatalogSO>();
            EditorGUILayout.LabelField($"GenericCardCatalogSO assets: {cats.Count}", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            foreach (var cat in cats)
            {
                int total = cat.Entries?.Count ?? 0;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(cat.name, EditorStyles.boldLabel, GUILayout.Width(280));
                        GUILayout.Label($"entries={total}", GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Ping", GUILayout.Width(48)))
                            EditorGUIUtility.PingObject(cat);
                    }
                    DrawEntryList(cat.Entries);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Shared entry-list renderer (Views 3 + 4)
        // ──────────────────────────────────────────────────────────────────
        private static void DrawEntryList(List<MusicianCardEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No entries.", MessageType.None);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                using (new EditorGUILayout.HorizontalScope())
                {
                    string id = e.card != null ? e.card.Id : "<null>";
                    GUILayout.Label($"[{i + 1}]", GUILayout.Width(34));
                    GUILayout.Label(id, GUILayout.Width(260));
                    GUILayout.Label(e.IsStarter ? $"S×{e.starterCopies}" : "—", GUILayout.Width(60));
                    GUILayout.Label(e.IsReward ? "R" : "—", GUILayout.Width(28));
                    GUILayout.Label(e.UnlockedByDefault ? "U" : "L", GUILayout.Width(28));
                    GUILayout.Label(string.IsNullOrEmpty(e.unlockId) ? "" : $"unlock={e.unlockId}");
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Print to Console
        // ──────────────────────────────────────────────────────────────────
        private void PrintCurrentView()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== CARD INVENTORY — {_view} ===");

            switch (_view)
            {
                case View.AllCardDefinitions:
                    foreach (var c in FindAllAssets<CardDefinition>())
                        sb.AppendLine($"  {c.Id} | {(c.IsAction ? "Action" : c.IsComposition ? "Composition" : "?")} | cost={c.InspirationCost} | {AssetDatabase.GetAssetPath(c)}");
                    break;
                case View.AllMusicianCatalogs:
                    foreach (var cat in FindAllAssets<MusicianCardCatalogData>())
                    {
                        int s = 0, sc = 0;
                        if (cat.Entries != null)
                            foreach (var e in cat.Entries)
                                if (e != null && e.IsStarter) { s++; sc += Mathf.Max(1, e.starterCopies); }
                        sb.AppendLine($"  {cat.MusicianType} | {cat.name} | entries={cat.Entries?.Count ?? 0} | starter={s} | copies={sc}");
                    }
                    break;
                case View.SingleMusicianCatalog:
                    foreach (var cat in FindAllAssets<MusicianCardCatalogData>())
                        if (cat.MusicianType == _selectedMusician) AppendEntries(sb, cat.name, cat.Entries);
                    break;
                case View.AllGenericCatalogs:
                    foreach (var cat in FindAllAssets<GenericCardCatalogSO>())
                        AppendEntries(sb, cat.name, cat.Entries);
                    break;
            }

            Debug.Log(sb.ToString());
        }

        private static void AppendEntries(StringBuilder sb, string title, List<MusicianCardEntry> entries)
        {
            sb.AppendLine($"  -- {title} --");
            if (entries == null) { sb.AppendLine("    (no entries)"); return; }
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) { sb.AppendLine($"    [{i + 1}] <null>"); continue; }
                string id = e.card != null ? e.card.Id : "<null>";
                sb.AppendLine($"    [{i + 1}] {id} | flags=[{e.flags}] | copies={e.starterCopies} | unlockId={(string.IsNullOrEmpty(e.unlockId) ? "<none>" : e.unlockId)}");
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Export JSON
        // ──────────────────────────────────────────────────────────────────
        [Serializable] private class JsonCardDef { public string id; public string displayName; public string kind; public int inspirationCost; public string assetPath; }
        [Serializable] private class JsonCatalogSummary { public string musicianType; public string assetName; public int entryCount; public int starterCount; public int starterCopiesTotal; }
        [Serializable] private class JsonEntry { public string cardId; public string flags; public int starterCopies; public string unlockId; }
        [Serializable] private class JsonCatalogFull { public string assetName; public string musicianType; public List<JsonEntry> entries = new(); }

        [Serializable] private class WrapDefs { public List<JsonCardDef> cardDefinitions = new(); }
        [Serializable] private class WrapCatSums { public List<JsonCatalogSummary> catalogs = new(); }
        [Serializable] private class WrapCatsFull { public List<JsonCatalogFull> catalogs = new(); }

        private void ExportCurrentView()
        {
            string defaultName = $"CardInventory_{_view}.json";
            string path = EditorUtility.SaveFilePanel("Export Card Inventory JSON", "", defaultName, "json");
            if (string.IsNullOrEmpty(path)) return;

            string json;
            switch (_view)
            {
                case View.AllCardDefinitions:
                    {
                        var w = new WrapDefs();
                        foreach (var c in FindAllAssets<CardDefinition>())
                            w.cardDefinitions.Add(new JsonCardDef
                            {
                                id = c.Id,
                                displayName = c.DisplayName,
                                kind = c.IsAction ? "Action" : c.IsComposition ? "Composition" : "?",
                                inspirationCost = c.InspirationCost,
                                assetPath = AssetDatabase.GetAssetPath(c)
                            });
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.AllMusicianCatalogs:
                    {
                        var w = new WrapCatSums();
                        foreach (var cat in FindAllAssets<MusicianCardCatalogData>())
                        {
                            int s = 0, sc = 0;
                            if (cat.Entries != null)
                                foreach (var e in cat.Entries)
                                    if (e != null && e.IsStarter) { s++; sc += Mathf.Max(1, e.starterCopies); }
                            w.catalogs.Add(new JsonCatalogSummary
                            {
                                musicianType = cat.MusicianType.ToString(),
                                assetName = cat.name,
                                entryCount = cat.Entries?.Count ?? 0,
                                starterCount = s,
                                starterCopiesTotal = sc
                            });
                        }
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.SingleMusicianCatalog:
                    {
                        var w = new WrapCatsFull();
                        foreach (var cat in FindAllAssets<MusicianCardCatalogData>())
                            if (cat.MusicianType == _selectedMusician)
                                w.catalogs.Add(BuildFullCatalog(cat.name, cat.MusicianType.ToString(), cat.Entries));
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                case View.AllGenericCatalogs:
                    {
                        var w = new WrapCatsFull();
                        foreach (var cat in FindAllAssets<GenericCardCatalogSO>())
                            w.catalogs.Add(BuildFullCatalog(cat.name, "<generic>", cat.Entries));
                        json = JsonUtility.ToJson(w, true);
                        break;
                    }
                default: json = "{}"; break;
            }

            File.WriteAllText(path, json);
            Debug.Log($"[CardInventory] Exported to {path}");
            EditorUtility.RevealInFinder(path);
        }

        private static JsonCatalogFull BuildFullCatalog(string name, string musician, List<MusicianCardEntry> entries)
        {
            var c = new JsonCatalogFull { assetName = name, musicianType = musician };
            if (entries == null) return c;
            foreach (var e in entries)
            {
                if (e == null) continue;
                c.entries.Add(new JsonEntry
                {
                    cardId = e.card != null ? e.card.Id : null,
                    flags = e.flags.ToString(),
                    starterCopies = e.starterCopies,
                    unlockId = e.unlockId
                });
            }
            return c;
        }

        // ──────────────────────────────────────────────────────────────────
        // Asset discovery
        // ──────────────────────────────────────────────────────────────────
        private static List<T> FindAllAssets<T>() where T : ScriptableObject
        {
            var list = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var a = AssetDatabase.LoadAssetAtPath<T>(path);
                if (a != null) list.Add(a);
            }
            return list;
        }
    }
}
#endif
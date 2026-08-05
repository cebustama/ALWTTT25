#if UNITY_EDITOR
using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
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
    ///
    /// AUTH-1 additions: "Detailed" print mode (per-card parameter dump — Action effects+params,
    /// Composition primaryKind/trackAction/bundle fields/partAction/modifierEffects, entry
    /// flags/copies/unlockId), cross-link Edit buttons into CardEditorWindow, and the shared
    /// CardAuthoringNav strip. Export JSON intentionally unchanged (contract untouched).
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

        // AUTH-1: detailed print toggle (affects Print output only).
        [SerializeField] private bool _detailedPrint = true;

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
            CardAuthoringNav.DrawNavStrip(CardAuthoringNav.Tool.Inventory);
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

                // AUTH-1: detailed print toggle
                _detailedPrint = GUILayout.Toggle(
                    _detailedPrint, "Detailed", EditorStyles.toolbarButton, GUILayout.Width(64));

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

                    // AUTH-1 cross-link: jump into the Card Editor at this card.
                    if (GUILayout.Button("Edit", GUILayout.Width(44)))
                        CardEditorWindow.OpenAndSelect(c);

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

                    GUILayout.FlexibleSpace();

                    // AUTH-1 cross-link: jump into the Card Editor at this card.
                    using (new EditorGUI.DisabledScope(e.card == null))
                    {
                        if (GUILayout.Button("Edit", GUILayout.Width(44)))
                            CardEditorWindow.OpenAndSelect(e.card);
                    }
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Print to Console
        // ──────────────────────────────────────────────────────────────────
        private void PrintCurrentView()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== CARD INVENTORY — {_view}{(_detailedPrint ? " (detailed)" : "")} ===");

            switch (_view)
            {
                case View.AllCardDefinitions:
                    foreach (var c in FindAllAssets<CardDefinition>())
                    {
                        sb.AppendLine($"  {c.Id} | {(c.IsAction ? "Action" : c.IsComposition ? "Composition" : "?")} | cost={c.InspirationCost} | {AssetDatabase.GetAssetPath(c)}");
                        if (_detailedPrint) AppendCardDetail(sb, c, "    ");
                    }
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
                        if (cat.MusicianType == _selectedMusician) AppendEntries(sb, cat.name, cat.Entries, _detailedPrint);
                    break;
                case View.AllGenericCatalogs:
                    foreach (var cat in FindAllAssets<GenericCardCatalogSO>())
                        AppendEntries(sb, cat.name, cat.Entries, _detailedPrint);
                    break;
            }

            Debug.Log(sb.ToString());
        }

        private static void AppendEntries(
            StringBuilder sb, string title, List<MusicianCardEntry> entries, bool detailed)
        {
            sb.AppendLine($"  -- {title} --");
            if (entries == null) { sb.AppendLine("    (no entries)"); return; }
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) { sb.AppendLine($"    [{i + 1}] <null>"); continue; }
                string id = e.card != null ? e.card.Id : "<null>";
                sb.AppendLine($"    [{i + 1}] {id} | flags=[{e.flags}] | copies={e.starterCopies} | unlockId={(string.IsNullOrEmpty(e.unlockId) ? "<none>" : e.unlockId)}");
                if (detailed && e.card != null)
                    AppendCardDetail(sb, e.card, "        ");
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // AUTH-1 — detailed per-card dump (Print only)
        // ──────────────────────────────────────────────────────────────────
        private static void AppendCardDetail(StringBuilder sb, CardDefinition c, string ind)
        {
            if (c == null) return;

            // Common CardDefinition surface
            string performer = c.RequiresFixedPerformer
                ? $"Fixed:{c.FixedPerformerType}"
                : c.PerformerRule.ToString();

            sb.AppendLine(
                $"{ind}name=\"{c.DisplayName}\" | performer={performer} | " +
                $"gen={c.InspirationGenerated} | rarity={c.Rarity} | type={c.CardType} | " +
                $"exhaust={c.ExhaustAfterPlay}");

            // Action side
            var ap = c.ActionPayload;
            if (ap != null)
            {
                sb.AppendLine(
                    $"{ind}ACTION | timing={ap.ActionTiming} | " +
                    $"conditions={ap.Conditions?.Count ?? 0} (legacy)");
                AppendEffects(sb, c.Payload.Effects, ind);
                return;
            }

            // Composition side
            var cp = c.CompositionPayload;
            if (cp != null)
            {
                sb.AppendLine($"{ind}COMPOSITION | primaryKind={cp.PrimaryKind}");

                var ta = cp.TrackAction;
                if (ta != null)
                {
                    ScriptableObject bundle = ta.styleBundle;
                    if (bundle == null)
                    {
                        sb.AppendLine($"{ind}track: role={ta.role} | bundle=<none>");
                    }
                    else
                    {
                        sb.AppendLine(
                            $"{ind}track: role={ta.role} | " +
                            $"bundle={bundle.name} ({bundle.GetType().Name})");
                        AppendSerializedFields(sb, bundle, ind + "    ");
                    }
                }

                var pa = cp.PartAction;
                if (pa != null)
                {
                    string label = string.IsNullOrEmpty(pa.customLabel) ? "<none>" : pa.customLabel;
                    string mus = string.IsNullOrEmpty(pa.musicianId) ? "<none>" : pa.musicianId;
                    sb.AppendLine($"{ind}part: action={pa.action} | label={label} | musicianId={mus}");
                }

                var mods = cp.ModifierEffects;
                if (mods != null && mods.Count > 0)
                {
                    sb.AppendLine($"{ind}modifiers ({mods.Count}):");
                    for (int i = 0; i < mods.Count; i++)
                    {
                        var m = mods[i];
                        if (m == null) { sb.AppendLine($"{ind}  [{i}] <null>"); continue; }
                        sb.AppendLine(
                            $"{ind}  [{i}] {m.GetType().Name} \"{m.name}\" | " +
                            $"scope={m.scope} | timing={m.timing} | {SafeLabel(m)}");
                    }
                }

                AppendEffects(sb, c.Payload.Effects, ind);
            }
        }

        private static void AppendEffects(
            StringBuilder sb, IReadOnlyList<CardEffectSpec> effects, string ind)
        {
            if (effects == null || effects.Count == 0)
            {
                sb.AppendLine($"{ind}effects: <none>");
                return;
            }

            sb.AppendLine($"{ind}effects ({effects.Count}):");
            for (int i = 0; i < effects.Count; i++)
                sb.AppendLine($"{ind}  [{i}] {DescribeSpec(effects[i])}");
        }

        /// <summary>Plain-text spec formatter for console output. Deliberately
        /// separate from CardEffectDescriptionBuilder, which emits TMP rich
        /// text for player-facing card descriptions. Unknown spec types fall
        /// back to the type name.</summary>
        private static string DescribeSpec(CardEffectSpec s)
        {
            switch (s)
            {
                case null:
                    return "<null spec>";

                case ApplyStatusEffectSpec a:
                    {
                        string st = a.status != null
                            ? (string.IsNullOrWhiteSpace(a.status.DisplayName)
                                ? a.status.name : a.status.DisplayName)
                            : "<null status>";
                        string delay = a.delay > 0f ? $" | delay={a.delay:0.##}s" : "";
                        return $"ApplyStatus \"{st}\" | stacks={Signed(a.stacksDelta)} | target={a.targetType}{delay}";
                    }

                case ModifyVibeSpec v:
                    return $"ModifyVibe {Signed(v.amount)} | target={v.targetType}";

                case ModifyStressSpec m:
                    return $"ModifyStress {Signed(m.amount)} | target={m.targetType}";

                case DrawCardsSpec d:
                    return $"DrawCards x{d.count}";

                case AddInspirationPerLoopSpec i:
                    return $"AddInspirationPerLoop +{i.amountPerLoop}/loop";

                default:
                    return s.GetType().Name;
            }
        }

        private static string Signed(int v) => v >= 0 ? $"+{v}" : v.ToString();

        private static string SafeLabel(PartEffect fx)
        {
            try { return fx != null ? fx.GetLabel() : ""; }
            catch { return "<label error>"; }
        }

        /// <summary>Depth-1 generic dump of an SO's visible serialized fields.
        /// Used for track style bundles (MidiGenPlay-owned types): read-only
        /// reflection over the serialized surface, no type coupling, bounded
        /// output (arrays print size only).</summary>
        private static void AppendSerializedFields(StringBuilder sb, ScriptableObject so, string ind)
        {
            var ser = new SerializedObject(so);
            var it = ser.GetIterator();
            bool enter = true;

            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.name == "m_Script") continue;
                sb.AppendLine($"{ind}{it.name} = {DescribeProp(it)}");
            }
        }

        private static string DescribeProp(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.intValue.ToString();
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.floatValue.ToString("0.###");
                case SerializedPropertyType.String: return $"\"{p.stringValue}\"";
                case SerializedPropertyType.Enum:
                    {
                        var names = p.enumNames;
                        int idx = p.enumValueIndex;
                        return idx >= 0 && idx < names.Length ? names[idx] : $"<enum {idx}>";
                    }
                case SerializedPropertyType.ObjectReference:
                    return p.objectReferenceValue != null
                        ? $"{p.objectReferenceValue.name} ({p.objectReferenceValue.GetType().Name})"
                        : "<null>";
                case SerializedPropertyType.Generic:
                    return p.isArray ? $"[{p.arraySize} items]" : "<struct>";
                default:
                    return $"<{p.propertyType}>";
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Export JSON  (unchanged by AUTH-1)
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
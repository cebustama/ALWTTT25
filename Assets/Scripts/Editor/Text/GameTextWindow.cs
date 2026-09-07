#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ALWTTT.Cards;
using ALWTTT.Status;
using ALWTTT.Tutorial;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    /// <summary>
    /// [TXT-1] Game Text window: one tab per text category, a table of
    /// element × language, inline editing, search, issue flags, and CSV export /
    /// import for the spreadsheet loop (D-TXT-1: authoring only, nothing at runtime).
    ///
    /// v1 status per tab:
    /// - Tutorial — FULL. Reads every TutorialDialogCatalogSO, one column per
    ///   languageCode (D2=B), edits revisitTitle + pages on the TutorialDialogSO
    ///   assets through SerializedObject (D1=A: assets stay the truth), creates a
    ///   missing dialog for parity repair, exports/imports CSV.
    /// - Cards — INVENTORY ONLY. CardDefinition.displayName is the only text field
    ///   and it has a single language; descriptions are generated in code by
    ///   CardEffectDescriptionBuilder (SSoT_Card_System §10.1). Editing deferred
    ///   (D-TXT-2: tutorial first; language slot = O-TXT-3).
    /// - Status effects — INVENTORY ONLY. StatusEffectSO.displayName + description,
    ///   single language. NOTE: editing displayName triggers the SO's OnValidate
    ///   auto-rename of the .asset file — a future edit tab must decide about that.
    /// - Menus — DECLARED UNREACHABLE. MainMenuController carries no strings; button
    ///   labels are TMP_Text components in the MainMenu scene/prefabs, not assets
    ///   discoverable by AssetDatabase type search.
    ///
    /// Style follows CardInventoryWindow / PartEffectEditorWindow (toolbar toggles,
    /// helpBox rows, Ping, SaveFilePanel + RevealInFinder export).
    /// </summary>
    public sealed class GameTextWindow : EditorWindow
    {
        private enum Tab { Tutorial, Cards, StatusEffects, Menus }

        [SerializeField] private Tab _tab = Tab.Tutorial;
        [SerializeField] private string _search = "";
        [SerializeField] private bool _onlyIssues;
        [SerializeField] private int _delimiterIndex; // index into GameTextCsv.SupportedDelimiters
        [SerializeField] private Vector2 _scroll;
        [SerializeField] private bool _showCatalogs = true;

        // ── Layout (TXT-1b) ──────────────────────────────────────────────
        // Columns split the available width evenly across the language count, so
        // two languages get half the window each instead of a fixed narrow column.
        // Page fields auto-grow to fit their text (CalcHeight at the real width),
        // clamped so one long page cannot push the rest of the table off-screen.
        private const float ColumnGap = 8f;
        private const float MinColumnWidth = 260f;
        private const float PageMinHeight = 46f;
        private const float PageMaxHeight = 260f;
        private const float IdColumnWidth = 260f;
        [SerializeField, Range(0.8f, 1.6f)] private float _density = 1f;

        private TutorialTextTable _tutorial;
        private string _lastFingerprint;

        private static GUIStyle _wrapArea;
        private static GUIStyle WrapArea => _wrapArea ??= new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            padding = new RectOffset(6, 6, 5, 5),
        };

        [MenuItem("ALWTTT/Text/Game Text", priority = 40)]
        public static void Open()
        {
            var w = GetWindow<GameTextWindow>();
            w.titleContent = new GUIContent("Game Text");
            w.minSize = new Vector2(760, 440);
            w.Show();
        }

        private void OnEnable()
        {
            Rebuild();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;
        private void OnProjectChange() => Rebuild();          // seeder re-run, asset added/removed
        private void OnUndoRedo() { _tutorial?.UpdateSerialized(); _tutorial?.RefreshIssues(); Repaint(); }

        private void Rebuild()
        {
            _tutorial ??= new TutorialTextTable();
            _tutorial.Rebuild();
        }

        // ──────────────────────────────────────────────────────────────────
        // GUI
        // ──────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (_tutorial == null) Rebuild();
            DrawToolbar();
            EditorGUILayout.Space(4);
            using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = s.scrollPosition;
                switch (_tab)
                {
                    case Tab.Tutorial: DrawTutorial(); break;
                    case Tab.Cards: DrawCards(); break;
                    case Tab.StatusEffects: DrawStatusEffects(); break;
                    case Tab.Menus: DrawMenus(); break;
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                TabToggle(Tab.Tutorial, "Tutorial", 80);
                TabToggle(Tab.Cards, "Cards", 60);
                TabToggle(Tab.StatusEffects, "Status Effects", 100);
                TabToggle(Tab.Menus, "Menus", 60);

                GUILayout.Space(8);
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(140), GUILayout.MaxWidth(260));
                if (_tab == Tab.Tutorial)
                {
                    _onlyIssues = GUILayout.Toggle(_onlyIssues, "Only issues", EditorStyles.toolbarButton, GUILayout.Width(84));
                    GUILayout.Label("Height", EditorStyles.miniLabel, GUILayout.Width(42));
                    _density = GUILayout.HorizontalSlider(_density, 0.8f, 1.6f, GUILayout.Width(70));
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label("CSV:", GUILayout.Width(30));
                _delimiterIndex = EditorGUILayout.Popup(_delimiterIndex, GameTextCsv.DelimiterLabels, EditorStyles.toolbarPopup, GUILayout.Width(110));
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(56))) ExportCsv();
                using (new EditorGUI.DisabledScope(_tab != Tab.Tutorial))
                    if (GUILayout.Button(new GUIContent("Import", _tab == Tab.Tutorial ? "Apply a CSV to the tutorial assets" : "Import is Tutorial-only in TXT-1"),
                            EditorStyles.toolbarButton, GUILayout.Width(56))) ImportCsv();

                if (_tab == Tab.Tutorial)
                {
                    if (GUILayout.Button("Fingerprint", EditorStyles.toolbarButton, GUILayout.Width(76))) PrintFingerprint();
                    if (GUILayout.Button("Parity", EditorStyles.toolbarButton, GUILayout.Width(50)))
                        EditorApplication.ExecuteMenuItem("ALWTTT/Tutorial/Validate catalog language parity");
                    int dirty = _tutorial.DirtyCount();
                    using (new EditorGUI.DisabledScope(dirty == 0))
                        if (GUILayout.Button(dirty == 0 ? "Saved" : $"Save ({dirty})", EditorStyles.toolbarButton, GUILayout.Width(72)))
                            AssetDatabase.SaveAssets();
                }
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(56))) Rebuild();
            }
        }

        private void TabToggle(Tab tab, string label, float width)
        {
            if (GUILayout.Toggle(_tab == tab, label, EditorStyles.toolbarButton, GUILayout.Width(width)))
            {
                if (_tab != tab) { _tab = tab; _scroll = Vector2.zero; }
            }
        }

        private bool Matches(params string[] haystack)
        {
            if (string.IsNullOrWhiteSpace(_search)) return true;
            foreach (var h in haystack)
                if (!string.IsNullOrEmpty(h) && h.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        // ──────────────────────────────────────────────────────────────────
        // Tab 1 — Tutorial (full)
        // ──────────────────────────────────────────────────────────────────
        private void DrawTutorial()
        {
            var t = _tutorial;
            t.UpdateSerialized();

            // Catalogs / languages header
            _showCatalogs = EditorGUILayout.Foldout(_showCatalogs,
                $"Catalogs: {t.Catalogs.Count} · Languages: {(t.Languages.Count == 0 ? "none" : string.Join(", ", t.Languages))} · Dialog rows: {t.Rows.Count}", true);
            if (_showCatalogs)
            {
                if (t.Catalogs.Count == 0)
                    EditorGUILayout.HelpBox("No TutorialDialogCatalogSO assets found.", MessageType.Warning);
                foreach (var c in t.Catalogs) DrawCatalogRow(c);
            }
            EditorGUILayout.Space(6);

            if (t.Languages.Count == 0)
            {
                EditorGUILayout.HelpBox("Set a languageCode on each catalog above to build the language columns.", MessageType.Info);
                return;
            }

            int shown = 0;
            foreach (var row in t.Rows)
            {
                if (_onlyIssues && !row.HasIssues) continue;
                if (!Matches(Concat(row))) continue;
                shown++;
                DrawDialogRow(row);
            }
            EditorGUILayout.LabelField($"{shown} / {t.Rows.Count} dialogs shown", EditorStyles.miniLabel);
        }

        private static string Concat(TutorialTextTable.Row row)
        {
            var sb = new StringBuilder(row.Id);
            foreach (var c in row.ByLanguage.Values)
            {
                if (c.Dialog == null) continue;
                sb.Append('\n').Append(c.Title);
                foreach (var p in c.Pages) sb.Append('\n').Append(p);
            }
            return sb.ToString();
        }

        private void DrawCatalogRow(TutorialTextTable.CatalogInfo c)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(c.Catalog.name, EditorStyles.boldLabel, GUILayout.Width(220));
                GUILayout.Label($"{c.Catalog.Dialogs.Count} dialogs", EditorStyles.miniLabel, GUILayout.Width(70));

                GUILayout.Label("lang:", GUILayout.Width(32));
                string code = EditorGUILayout.DelayedTextField(c.Language, GUILayout.Width(44));
                if (code != c.Language) { SetCatalogLanguage(c, code); return; }
                if (string.IsNullOrEmpty(c.Language))
                {
                    string suggestion = TutorialTextTable.SuggestLanguageFromAssetName(c.Catalog.name);
                    if (!string.IsNullOrEmpty(suggestion) && GUILayout.Button($"Use '{suggestion}'", GUILayout.Width(70)))
                    { SetCatalogLanguage(c, suggestion); return; }
                    Badge("NO LANGUAGE", Color.yellow);
                }
                if (c.DuplicateLanguage) Badge("DUPLICATE (ignored)", Color.red);

                GUILayout.FlexibleSpace();
                if (c.Parity.Ok) Badge("PARITY OK", new Color(0.55f, 0.85f, 0.55f));
                else Badge($"PARITY missing {c.Parity.Missing.Count} · extra {c.Parity.Extra.Count}", new Color(0.95f, 0.6f, 0.5f));
                if (GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(c.Catalog);
            }
        }

        private void SetCatalogLanguage(TutorialTextTable.CatalogInfo c, string code)
        {
            _tutorial.SetCatalogLanguage(c, code);
            Rebuild();
            GUIUtility.ExitGUI();
        }

        private void DrawDialogRow(TutorialTextTable.Row row)
        {
            var t = _tutorial;

            // Width available to the row: the view minus the vertical scrollbar and the
            // helpBox margins. currentViewWidth is the only width known during Layout,
            // so column sizing has to come from it rather than from a Rect.
            float avail = EditorGUIUtility.currentViewWidth - 34f;
            int n = Mathf.Max(1, t.Languages.Count);
            float col = Mathf.Max(MinColumnWidth, (avail - (n - 1) * ColumnGap) / n);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.SelectableLabel(row.Id, EditorStyles.boldLabel,
                        GUILayout.Width(IdColumnWidth), GUILayout.Height(18));
                    string meta = row.Priority == int.MaxValue ? "unauthored"
                        : $"prio {row.Priority} · {row.Category}" + (string.IsNullOrEmpty(row.HighlightKey) ? "" : $" · ⌖ {row.HighlightKey}");
                    GUILayout.Label(meta, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    foreach (var issue in row.Issues)
                        Badge(issue, issue.StartsWith("MISSING") || issue.StartsWith("EXTRA") ? new Color(0.95f, 0.6f, 0.5f) : Color.yellow);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int i = 0; i < t.Languages.Count; i++)
                    {
                        string lang = t.Languages[i];
                        if (i > 0) GUILayout.Space(ColumnGap);
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(col)))
                        {
                            row.ByLanguage.TryGetValue(lang, out var cell);
                            DrawCell(row, lang, cell, col);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawCell(TutorialTextTable.Row row, string lang, TutorialTextTable.Cell cell, float colWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(lang.ToUpperInvariant(), EditorStyles.miniBoldLabel, GUILayout.Width(28));
                GUILayout.FlexibleSpace();
                if (cell?.Dialog != null && GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(40)))
                    EditorGUIUtility.PingObject(cell.Dialog);
            }

            if (cell == null || cell.Dialog == null)
            {
                EditorGUILayout.HelpBox($"No dialog in '{lang}'.", MessageType.Warning);
                if (GUILayout.Button($"Create {row.Id} [{lang}]"))
                {
                    if (!_tutorial.CreateMissingDialog(row, lang, out string err))
                        EditorUtility.DisplayDialog("Create dialog", err, "OK");
                    Rebuild();
                    GUIUtility.ExitGUI();
                }
                return;
            }

            var so = cell.Serialized;
            so.Update();
            EditorGUI.BeginChangeCheck();

            // Title on its own full-width line: a label + field on one line squeezed the
            // field down to a few characters at these column widths.
            var title = so.FindProperty("revisitTitle");
            GUILayout.Label("Title", EditorStyles.miniLabel);
            title.stringValue = EditorGUILayout.TextField(title.stringValue, GUILayout.Width(colWidth - 6f));

            var pages = so.FindProperty("pages");
            float textWidth = colWidth - 60f;   // P-label + remove button + padding
            for (int i = 0; i < pages.arraySize; i++)
            {
                var p = pages.GetArrayElementAtIndex(i);
                float h = Mathf.Clamp(
                    WrapArea.CalcHeight(new GUIContent(p.stringValue ?? ""), textWidth) + 4f,
                    PageMinHeight, PageMaxHeight) * _density;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"P{i + 1}", GUILayout.Width(24));
                    p.stringValue = EditorGUILayout.TextArea(p.stringValue, WrapArea,
                        GUILayout.Width(textWidth), GUILayout.Height(h));
                    if (pages.arraySize > 1 && GUILayout.Button("−", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        pages.DeleteArrayElementAtIndex(i);
                        so.ApplyModifiedProperties();
                        _tutorial.RefreshIssues();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ page", EditorStyles.miniButton, GUILayout.Width(56)))
                {
                    pages.arraySize++;
                    pages.GetArrayElementAtIndex(pages.arraySize - 1).stringValue = "";
                }
                GUILayout.Space(24);
            }

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();   // marks dirty + Undo; Save button / Ctrl+S persists to disk
                _tutorial.RefreshIssues();
            }
        }

        private static void Badge(string text, Color color)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUILayout.Label(text, EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            GUI.color = prev;
        }

        // ──────────────────────────────────────────────────────────────────
        // Tab 2 — Cards (inventory only)
        // ──────────────────────────────────────────────────────────────────
        private void DrawCards()
        {
            var defs = FindAllAssets<CardDefinition>();
            EditorGUILayout.HelpBox(
                "Inventory only (TXT-1 / D-TXT-2). Text field: CardDefinition.displayName — one language, no description field. " +
                "Card descriptions are generated in code by CardEffectDescriptionBuilder (SSoT_Card_System §10.1) and are not editable here. " +
                "Editing + a language slot are deferred to the O-TXT-3 decision.", MessageType.Info);
            EditorGUILayout.LabelField($"CardDefinition assets: {defs.Count}", EditorStyles.boldLabel);
            foreach (var c in defs.OrderBy(d => d.Id, StringComparer.Ordinal))
            {
                if (!Matches(c.Id, c.DisplayName)) continue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.SelectableLabel(c.Id, GUILayout.Width(220), GUILayout.Height(18));
                    EditorGUILayout.SelectableLabel(c.DisplayName ?? "", GUILayout.Height(18));
                    if (string.IsNullOrWhiteSpace(c.DisplayName)) Badge("EMPTY", Color.yellow);
                    if (GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(c);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Tab 3 — Status effects (inventory only)
        // ──────────────────────────────────────────────────────────────────
        private void DrawStatusEffects()
        {
            var fx = FindAllAssets<StatusEffectSO>();
            EditorGUILayout.HelpBox(
                "Inventory only (TXT-1 / D-TXT-2). Text fields: StatusEffectSO.displayName + description — one language. " +
                "Caution for the future edit tab: changing displayName triggers the SO's OnValidate auto-rename of the .asset file " +
                "(StatusEffect_{DisplayName}_{EffectId}).", MessageType.Info);
            EditorGUILayout.LabelField($"StatusEffectSO assets: {fx.Count}", EditorStyles.boldLabel);
            foreach (var s in fx.OrderBy(x => x.StatusKey, StringComparer.Ordinal))
            {
                if (!Matches(s.StatusKey, s.DisplayName, s.Description)) continue;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.SelectableLabel(s.StatusKey ?? "", EditorStyles.boldLabel, GUILayout.Width(180), GUILayout.Height(18));
                        EditorGUILayout.SelectableLabel(s.DisplayName ?? "", GUILayout.Height(18));
                        if (string.IsNullOrWhiteSpace(s.Description)) Badge("NO DESCRIPTION", Color.yellow);
                        if (GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(s);
                    }
                    if (!string.IsNullOrWhiteSpace(s.Description))
                        EditorGUILayout.LabelField(s.Description, EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Tab 4 — Menus (declared unreachable in v1)
        // ──────────────────────────────────────────────────────────────────
        private void DrawMenus()
        {
            EditorGUILayout.HelpBox(
                "Not reachable in TXT-1. MainMenuController.cs carries no player-facing strings; menu button labels are TMP_Text " +
                "components serialized in the MainMenu scene / prefabs. They are not ScriptableObject assets, so AssetDatabase type " +
                "search cannot enumerate them and this window cannot edit them without a scene-walking pass. " +
                "Options scene: not verified. Decide the storage model (O-TXT-3) before adding this tab.", MessageType.Warning);
        }

        // ──────────────────────────────────────────────────────────────────
        // CSV
        // ──────────────────────────────────────────────────────────────────
        private char Delimiter => GameTextCsv.SupportedDelimiters[Mathf.Clamp(_delimiterIndex, 0, GameTextCsv.SupportedDelimiters.Length - 1)];

        private void ExportCsv()
        {
            GameTextCsv.Table table;
            string suffix;
            switch (_tab)
            {
                case Tab.Tutorial:
                    table = _tutorial.ToCsvTable(); suffix = "Tutorial"; break;
                case Tab.Cards:
                    table = new GameTextCsv.Table { Header = { "id", "field", "en" } };
                    foreach (var c in FindAllAssets<CardDefinition>().OrderBy(d => d.Id, StringComparer.Ordinal))
                        table.Rows.Add(new[] { c.Id ?? "", "displayName", c.DisplayName ?? "" });
                    suffix = "Cards"; break;
                case Tab.StatusEffects:
                    table = new GameTextCsv.Table { Header = { "id", "field", "en" } };
                    foreach (var s in FindAllAssets<StatusEffectSO>().OrderBy(x => x.StatusKey, StringComparer.Ordinal))
                    {
                        table.Rows.Add(new[] { s.StatusKey ?? "", "displayName", s.DisplayName ?? "" });
                        table.Rows.Add(new[] { s.StatusKey ?? "", "description", s.Description ?? "" });
                    }
                    suffix = "StatusEffects"; break;
                default:
                    EditorUtility.DisplayDialog("Export CSV", "Nothing to export on this tab.", "OK"); return;
            }

            string path = EditorUtility.SaveFilePanel("Export Game Text CSV", "",
                $"GameText_{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.csv", "csv");
            if (string.IsNullOrEmpty(path)) return;
            GameTextCsv.Write(path, table, Delimiter);
            Debug.Log($"[GameText] Exported {table.Rows.Count} rows ({suffix}) → {path}");
            EditorUtility.RevealInFinder(path);
        }

        private void ImportCsv()
        {
            if (_tab != Tab.Tutorial) return;
            string path = EditorUtility.OpenFilePanel("Import Game Text CSV (Tutorial)", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            GameTextCsv.Table table;
            try { table = GameTextCsv.Read(path); }
            catch (FormatException ex)
            {
                EditorUtility.DisplayDialog("Import CSV", $"Malformed CSV — nothing applied.\n{ex.Message}", "OK");
                return;
            }

            string before = _tutorial.Fingerprint();
            var result = _tutorial.ApplyCsv(table);
            if (result.Aborted)
            {
                EditorUtility.DisplayDialog("Import CSV", "Import aborted — nothing applied:\n" + string.Join("\n", result.Errors), "OK");
                return;
            }
            string after = _tutorial.Fingerprint();

            var sb = new StringBuilder();
            sb.AppendLine($"[GameText] Import '{Path.GetFileName(path)}': {result.FieldsChanged} field(s) changed on {result.DialogsChanged} dialog(s); {result.DialogsUntouched} dialog(s) untouched.");
            sb.AppendLine($"  fingerprint before {before}");
            sb.AppendLine($"  fingerprint after  {after}{(before == after ? "  (identical — round-trip clean)" : "")}");
            if (result.Skipped.Count > 0)
            {
                sb.AppendLine($"  skipped ({result.Skipped.Count}):");
                foreach (var s in result.Skipped) sb.Append("    - ").AppendLine(s);
            }
            if (result.Skipped.Count > 0) Debug.LogWarning(sb.ToString()); else Debug.Log(sb.ToString());
            Rebuild();
        }

        private void PrintFingerprint()
        {
            string fp = _tutorial.Fingerprint();
            string note = _lastFingerprint == null ? "" : fp == _lastFingerprint ? " (unchanged since last print)" : " (CHANGED since last print)";
            Debug.Log($"[GameText] Tutorial text fingerprint: {fp}{note} — {_tutorial.Rows.Count} ids × {_tutorial.Languages.Count} languages");
            _lastFingerprint = fp;
        }

        // ──────────────────────────────────────────────────────────────────
        private static List<T> FindAllAssets<T>() where T : ScriptableObject
        {
            var list = new List<T>();
            foreach (var g in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null) list.Add(a);
            }
            return list;
        }
    }
}
#endif
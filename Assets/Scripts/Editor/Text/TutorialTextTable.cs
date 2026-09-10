#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ALWTTT.Tutorial;
using ALWTTT.Tooltips;   // [TXT-2] ConceptTagRenderer (tag regex + balance check)
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    /// <summary>
    /// [TXT-1] In-memory view of the tutorial copy across every
    /// <see cref="TutorialDialogCatalogSO"/> in the project, one column per
    /// <c>languageCode</c> (D2=B). Owns NO text: every read and write goes through
    /// a <see cref="SerializedObject"/> on the real <see cref="TutorialDialogSO"/>
    /// asset (D1=A — assets are the truth; this class is a lens plus a diff engine).
    ///
    /// Why SerializedObject and not new setters on the SO: the fields are private
    /// serialized fields; SerializedObject edits give Undo, dirty-marking and
    /// prefab-safe writes for free, and keep the SO's runtime surface untouched.
    /// </summary>
    internal sealed class TutorialTextTable
    {
        // ── Serialized field names on the SOs (change here if the SO changes) ──
        private const string PropTitle = "revisitTitle";
        private const string PropPages = "pages";
        private const string PropMechanic = "mechanicText";   // [TUT-TXT-1]
        private const string PropDialogs = "dialogs";
        private const string PropLanguage = "languageCode";

        /// <summary>D-S5f-5=B authoring cap: more than this many pages is flagged.</summary>
        public const int PageCap = 2;

        private static readonly Regex TokenRx = new(@"\{\$[A-Za-z0-9_]+\}", RegexOptions.Compiled);

        public sealed class CatalogInfo
        {
            public TutorialDialogCatalogSO Catalog;
            public SerializedObject Serialized;
            public string Path;
            public string Language;          // normalised languageCode ("" if unset)
            public bool DuplicateLanguage;   // another catalog already claims this code
            public TutorialDialogCatalogSO.ParityReport Parity;
        }

        public sealed class Cell
        {
            public TutorialDialogSO Dialog;      // null → MISSING in this language
            public SerializedObject Serialized;  // null when Dialog is null
            public string Title => Serialized?.FindProperty(PropTitle).stringValue ?? "";

            // [TUT-TXT-1] Null-safe on the property: a dialog asset serialized before step 1
            // still reads as "" instead of throwing.
            public string Mechanic => Serialized?.FindProperty(PropMechanic)?.stringValue ?? "";

            public List<string> Pages
            {
                get
                {
                    var list = new List<string>();
                    if (Serialized == null) return list;
                    var p = Serialized.FindProperty(PropPages);
                    for (int i = 0; i < p.arraySize; i++) list.Add(p.GetArrayElementAtIndex(i).stringValue ?? "");
                    return list;
                }
            }
        }

        public sealed class Row
        {
            public string Id;
            public bool Canonical;               // false → EXTRA (present in a catalog, not in TutorialTriggerId)
            public int Priority;                 // from the first dialog found (identical across languages by contract)
            public TutorialCategory Category;
            public string HighlightKey = "";
            public readonly Dictionary<string, Cell> ByLanguage = new();

            // Derived issue flags (recomputed by Refresh)
            public readonly List<string> Issues = new();
            public bool HasIssues => Issues.Count > 0;
        }

        public readonly List<CatalogInfo> Catalogs = new();
        /// <summary>Distinct language codes in column order ("en" first, then alphabetical).</summary>
        public readonly List<string> Languages = new();
        public readonly List<Row> Rows = new();

        // ──────────────────────────────────────────────────────────────────
        // Build
        // ──────────────────────────────────────────────────────────────────
        public void Rebuild()
        {
            Catalogs.Clear(); Languages.Clear(); Rows.Clear();
            ConceptRegistryEditorIndex.Build();   // [TXT-2] registries for the TAG unknown check

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(TutorialDialogCatalogSO)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var cat = AssetDatabase.LoadAssetAtPath<TutorialDialogCatalogSO>(path);
                if (cat == null) continue;
                var so = new SerializedObject(cat);
                var info = new CatalogInfo
                {
                    Catalog = cat,
                    Serialized = so,
                    Path = path,
                    Language = NormaliseLanguage(so.FindProperty(PropLanguage).stringValue),
                    Parity = TutorialDialogCatalogSO.ComputeParity(cat),
                };
                Catalogs.Add(info);
            }
            Catalogs.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));

            // Languages: first catalog per code wins; later ones are flagged as duplicates.
            var claimed = new HashSet<string>();
            foreach (var c in Catalogs)
            {
                if (string.IsNullOrEmpty(c.Language)) continue;
                if (!claimed.Add(c.Language)) { c.DuplicateLanguage = true; continue; }
                Languages.Add(c.Language);
            }
            Languages.Sort((a, b) =>
                a == "en" ? -1 : b == "en" ? 1 : string.CompareOrdinal(a, b));

            // Rows: canonical ids first (all of them, even if unauthored), then extras.
            var byId = new Dictionary<string, Row>();
            foreach (var id in TutorialDialogCatalogSO.CanonicalTriggerIds())
                byId[id] = new Row { Id = id, Canonical = true, Priority = int.MaxValue };

            foreach (var c in Catalogs)
            {
                if (string.IsNullOrEmpty(c.Language) || c.DuplicateLanguage) continue;
                foreach (var d in c.Catalog.Dialogs)
                {
                    if (d == null || string.IsNullOrEmpty(d.TriggerId)) continue;
                    if (!byId.TryGetValue(d.TriggerId, out var row))
                        byId[d.TriggerId] = row = new Row { Id = d.TriggerId, Canonical = false, Priority = int.MaxValue };
                    if (row.Priority == int.MaxValue)
                    {
                        row.Priority = d.Priority; row.Category = d.Category; row.HighlightKey = d.HighlightKey ?? "";
                    }
                    // First dialog per (id, language) wins; duplicates inside one catalog are a parity EXTRA anyway.
                    if (!row.ByLanguage.ContainsKey(c.Language))
                        row.ByLanguage[c.Language] = new Cell { Dialog = d, Serialized = new SerializedObject(d) };
                }
            }

            Rows.AddRange(byId.Values);
            Rows.Sort((a, b) =>
            {
                int p = a.Priority.CompareTo(b.Priority);
                return p != 0 ? p : string.CompareOrdinal(a.Id, b.Id);
            });
            RefreshIssues();
        }

        /// <summary>Pull latest serialized state (after Undo, seeder re-run, inspector edits).</summary>
        public void UpdateSerialized()
        {
            foreach (var c in Catalogs) c.Serialized.Update();
            foreach (var r in Rows) foreach (var cell in r.ByLanguage.Values) cell.Serialized?.Update();
        }

        public void RefreshIssues()
        {
            foreach (var row in Rows)
            {
                row.Issues.Clear();
                if (!row.Canonical) row.Issues.Add("EXTRA id");
                HashSet<string> refTokens = null;

                // [TUT-TXT-1 / D-TT-4=A] Mechanic text is flagged only on ASYMMETRY (one language
                // has it, the other does not): ids outside the D-TT-3 scope are legitimately empty
                // in every language and must not light up "Only issues".
                string refMech = null, refMechLang = null;
                HashSet<string> refMechTokens = null;
                // [TXT-2 / D-TAG-6] Concept tags are checked like tokens: the SET of ids must match
                // across languages (a translation that drops a tag is a UI hole the copy hides), and
                // every id must exist in some registry (status key, keyword, glossary).
                HashSet<string> refTags = null, refMechTags = null;

                foreach (var lang in Languages)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var cell) || cell.Dialog == null)
                    {
                        row.Issues.Add($"MISSING {lang}"); continue;
                    }
                    var pages = cell.Pages;
                    if (pages.Count == 0 || pages.Any(string.IsNullOrWhiteSpace)) row.Issues.Add($"EMPTY {lang}");
                    if (pages.Count > PageCap) row.Issues.Add($"PAGES>{PageCap} {lang}");
                    if (string.IsNullOrWhiteSpace(cell.Title)) row.Issues.Add($"NO TITLE {lang}");


                    var mechTokens = new HashSet<string>(TokenRx.Matches(cell.Mechanic ?? "").Cast<Match>().Select(m => m.Value));
                    if (refMechLang == null) { refMech = cell.Mechanic; refMechLang = lang; refMechTokens = mechTokens; }
                    else
                    {
                        bool hereEmpty = string.IsNullOrWhiteSpace(cell.Mechanic);
                        bool refEmpty = string.IsNullOrWhiteSpace(refMech);
                        if (hereEmpty != refEmpty) row.Issues.Add($"NO MECHANIC {(hereEmpty ? lang : refMechLang)}");
                        else if (!hereEmpty && !refMechTokens.SetEquals(mechTokens)) row.Issues.Add("MECHANIC TOKENS differ");
                    }

                    var tokens = new HashSet<string>(pages.SelectMany(p => TokenRx.Matches(p ?? "").Cast<Match>().Select(m => m.Value)));
                    if (refTokens == null) refTokens = tokens;
                    else if (!refTokens.SetEquals(tokens)) row.Issues.Add("TOKENS differ");

                    // [TXT-2] tag parity + unknown + unclosed (mechanic and pages)
                    var mechTags = new HashSet<string>(ConceptTagRenderer.ExtractIds(cell.Mechanic), StringComparer.OrdinalIgnoreCase);
                    var pageTags = new HashSet<string>(pages.SelectMany(ConceptTagRenderer.ExtractIds), StringComparer.OrdinalIgnoreCase);
                    if (refMechTags == null) refMechTags = mechTags;
                    else if (!string.IsNullOrWhiteSpace(cell.Mechanic) && !refMechTags.SetEquals(mechTags)) row.Issues.Add("MECHANIC TAGS differ");
                    if (refTags == null) refTags = pageTags;
                    else if (!refTags.SetEquals(pageTags)) row.Issues.Add("TAGS differ");
                    foreach (var id in mechTags.Concat(pageTags).Distinct(StringComparer.OrdinalIgnoreCase))
                        if (!ConceptRegistryEditorIndex.IsKnown(id)) row.Issues.Add($"TAG unknown: {id}");
                    if (ConceptTagRenderer.HasUnbalancedLinks(cell.Mechanic) || pages.Any(ConceptTagRenderer.HasUnbalancedLinks))
                        row.Issues.Add($"TAG unclosed {lang}");
                }
            }
        }

        public static string NormaliseLanguage(string raw) =>
            string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim().ToLowerInvariant();

        /// <summary>"TutorialDialogCatalog_EN" → "en". Empty when no suffix is found.</summary>
        public static string SuggestLanguageFromAssetName(string assetName)
        {
            int us = assetName.LastIndexOf('_');
            if (us < 0 || us == assetName.Length - 1) return "";
            string suffix = assetName.Substring(us + 1);
            return suffix.Length <= 5 && suffix.All(char.IsLetter) ? suffix.ToLowerInvariant() : "";
        }

        public void SetCatalogLanguage(CatalogInfo c, string code)
        {
            c.Serialized.Update();
            c.Serialized.FindProperty(PropLanguage).stringValue = NormaliseLanguage(code);
            c.Serialized.ApplyModifiedProperties();
        }

        // ──────────────────────────────────────────────────────────────────
        // Create a missing dialog (parity repair)
        // ──────────────────────────────────────────────────────────────────
        /// <summary>
        /// Creates <c>{dir}/{id}.asset</c> in the target catalog's dialog directory,
        /// copying priority / category / highlightKey from a sibling language (they are
        /// identical across languages by contract), with an empty title and one empty
        /// page, and appends it to the catalog. The seeders are NOT involved (D-TXT-3).
        /// </summary>
        public bool CreateMissingDialog(Row row, string lang, out string error)
        {
            error = null;
            var info = Catalogs.FirstOrDefault(c => c.Language == lang && !c.DuplicateLanguage);
            if (info == null) { error = $"No catalog with languageCode '{lang}'."; return false; }

            string dir = GuessDialogDirectory(info);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); AssetDatabase.Refresh(); }
            string path = $"{dir}/{row.Id}.asset";
            if (AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(path) != null)
            {
                error = $"'{path}' already exists but is not listed in '{info.Catalog.name}'. Add it to the catalog manually.";
                return false;
            }

            var sibling = row.ByLanguage.Values.FirstOrDefault(c => c.Dialog != null)?.Dialog;
            int prio = sibling != null ? sibling.Priority : 100;
            var cat = sibling != null ? sibling.Category : TutorialCategory.Cards;
            string highlight = sibling != null ? sibling.HighlightKey ?? "" : "";

            var so = ScriptableObject.CreateInstance<TutorialDialogSO>();
            AssetDatabase.CreateAsset(so, path);
            so.EditorSeed(row.Id, prio, cat, "", highlight, "");
            EditorUtility.SetDirty(so);

            info.Serialized.Update();
            var list = info.Serialized.FindProperty(PropDialogs);
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = so;
            info.Serialized.ApplyModifiedProperties();
            info.Catalog.BuildIndex();
            AssetDatabase.SaveAssets();
            return true;
        }

        private static string GuessDialogDirectory(CatalogInfo info)
        {
            // Most common directory among the catalog's existing dialogs; fallback keeps the
            // seeders' convention (Dialogs/ for en, Dialogs/<CODE>/ otherwise).
            var dirs = info.Catalog.Dialogs.Where(d => d != null)
                .Select(d => Path.GetDirectoryName(AssetDatabase.GetAssetPath(d))?.Replace('\\', '/'))
                .Where(p => !string.IsNullOrEmpty(p))
                .GroupBy(p => p).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(dirs)) return dirs;
            return info.Language == "en"
                ? "Assets/Resources/Data/Tutorial/Dialogs"
                : $"Assets/Resources/Data/Tutorial/Dialogs/{info.Language.ToUpperInvariant()}";
        }

        // ──────────────────────────────────────────────────────────────────
        // Export / Import
        // ──────────────────────────────────────────────────────────────────
        public GameTextCsv.Table ToCsvTable(Func<Row, bool> filter = null)
        {
            var t = new GameTextCsv.Table();
            t.Header.Add(GameTextCsv.ColId);
            t.Header.Add(GameTextCsv.ColField);
            t.Header.AddRange(Languages);

            foreach (var row in Rows)
            {
                if (filter != null && !filter(row)) continue;
                int maxPages = 0;
                foreach (var lang in Languages)
                    if (row.ByLanguage.TryGetValue(lang, out var c) && c.Dialog != null)
                        maxPages = Math.Max(maxPages, c.Pages.Count);

                t.Rows.Add(BuildRecord(row, GameTextCsv.FieldTitle, lang =>
                    row.ByLanguage.TryGetValue(lang, out var c) && c.Dialog != null ? c.Title : ""));

                // [TUT-TXT-1] Row order title → mechanicText → page_N is fixed: stable diffs.
                t.Rows.Add(BuildRecord(row, GameTextCsv.FieldMechanic, lang =>
                    row.ByLanguage.TryGetValue(lang, out var c) && c.Dialog != null ? c.Mechanic : ""));

                for (int p = 1; p <= maxPages; p++)
                {
                    int idx = p - 1;
                    t.Rows.Add(BuildRecord(row, GameTextCsv.PageField(p), lang =>
                    {
                        if (!row.ByLanguage.TryGetValue(lang, out var c) || c.Dialog == null) return "";
                        var pages = c.Pages;
                        return idx < pages.Count ? pages[idx] : "";
                    }));
                }
            }
            return t;
        }

        private string[] BuildRecord(Row row, string field, Func<string, string> cellFor)
        {
            var rec = new string[2 + Languages.Count];
            rec[0] = row.Id; rec[1] = field;
            for (int i = 0; i < Languages.Count; i++) rec[2 + i] = cellFor(Languages[i]) ?? "";
            return rec;
        }

        public sealed class ImportResult
        {
            public int DialogsChanged, FieldsChanged, DialogsUntouched;
            public readonly List<string> Skipped = new();   // human-readable reasons
            public readonly List<string> Errors = new();    // header-level problems → nothing applied
            public bool Aborted => Errors.Count > 0;
        }

        /// <summary>
        /// Applies a CSV table to the assets. Writes ONLY the (dialog, field) pairs whose
        /// value differs, so re-importing an unmodified export touches nothing — that is
        /// the round-trip contract. Partial CSVs are allowed: ids or languages absent from
        /// the file are left as they are. Unknown language columns abort the import.
        /// </summary>
        public ImportResult ApplyCsv(GameTextCsv.Table table)
        {
            var result = new ImportResult();
            if (table.Header.Count < 2 ||
                !string.Equals(table.Header[0].Trim(), GameTextCsv.ColId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(table.Header[1].Trim(), GameTextCsv.ColField, StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add($"Header must start with '{GameTextCsv.ColId}', '{GameTextCsv.ColField}'. Got: [{string.Join(" | ", table.Header)}]");
                return result;
            }
            var langCols = new Dictionary<string, int>();
            for (int i = 2; i < table.Header.Count; i++)
            {
                string code = NormaliseLanguage(table.Header[i]);
                if (string.IsNullOrEmpty(code)) continue; // trailing empty column (sheet artefact) → ignore
                if (!Languages.Contains(code)) { result.Errors.Add($"Unknown language column '{table.Header[i]}'. Known: {string.Join(", ", Languages)}."); continue; }
                langCols[code] = i;
            }
            if (result.Aborted) return result;

            // Group rows by id → field → lang → value
            var pending = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            for (int r = 0; r < table.Rows.Count; r++)
            {
                var rec = table.Rows[r];
                if (rec.Length < 2) continue;
                string id = rec[0].Trim(); string field = rec[1].Trim();
                if (string.IsNullOrEmpty(id)) continue;
                if (field != GameTextCsv.FieldTitle && field != GameTextCsv.FieldMechanic && GameTextCsv.ParsePageIndex(field) < 0)
                { result.Skipped.Add($"row {r + 2}: unknown field '{field}'"); continue; }
                if (!pending.TryGetValue(id, out var byField)) pending[id] = byField = new();
                if (!byField.TryGetValue(field, out var byLang)) byField[field] = byLang = new();
                foreach (var kv in langCols)
                    byLang[kv.Key] = kv.Value < rec.Length ? rec[kv.Value] ?? "" : "";
            }

            var rowsById = Rows.ToDictionary(x => x.Id);
            foreach (var kvId in pending)
            {
                if (!rowsById.TryGetValue(kvId.Key, out var row))
                { result.Skipped.Add($"'{kvId.Key}': id not in any catalog (create it in the window first)"); continue; }

                foreach (var lang in langCols.Keys)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var cell) || cell.Dialog == null)
                    { result.Skipped.Add($"'{row.Id}' [{lang}]: no dialog asset in this language (create it in the window first)"); continue; }

                    // Assemble the target state for this (id, lang)
                    string newTitle = null;
                    if (kvId.Value.TryGetValue(GameTextCsv.FieldTitle, out var tl) && tl.TryGetValue(lang, out var tv)) newTitle = tv;
                    string newMechanic = null;   // [TUT-TXT-1] same "empty cell = no opinion" rule as the title
                    if (kvId.Value.TryGetValue(GameTextCsv.FieldMechanic, out var ml) && ml.TryGetValue(lang, out var mv)) newMechanic = mv;

                    var pageCells = kvId.Value
                        .Where(f => GameTextCsv.ParsePageIndex(f.Key) > 0)
                        .Select(f => (idx: GameTextCsv.ParsePageIndex(f.Key), val: f.Value.TryGetValue(lang, out var pv) ? pv : ""))
                        .OrderBy(x => x.idx).ToList();
                    List<string> newPages = null;
                    if (pageCells.Count > 0)
                    {
                        // Contiguity: page_1..page_N must all be present as rows.
                        if (pageCells.Select(x => x.idx).SequenceEqual(Enumerable.Range(1, pageCells.Count)))
                        {
                            newPages = pageCells.Select(x => x.val).ToList();
                            while (newPages.Count > 0 && string.IsNullOrEmpty(newPages[^1])) newPages.RemoveAt(newPages.Count - 1); // trailing empties trim
                            if (newPages.Any(string.IsNullOrEmpty))
                            { result.Skipped.Add($"'{row.Id}' [{lang}]: empty page in the middle of the page list — pages not applied"); newPages = null; }
                            else if (newPages.Count == 0)
                            { result.Skipped.Add($"'{row.Id}' [{lang}]: all pages empty — pages not applied"); newPages = null; }
                        }
                        else result.Skipped.Add($"'{row.Id}': page rows are not contiguous (page_1..page_N) — pages not applied");
                    }

                    // Diff & write
                    cell.Serialized.Update();
                    bool touched = false;
                    // Empty title cell = "no opinion", never "clear the title" (blank sheet cells are usually accidents).
                    if (!string.IsNullOrEmpty(newTitle) && newTitle != cell.Title)
                    { cell.Serialized.FindProperty(PropTitle).stringValue = newTitle; touched = true; result.FieldsChanged++; }
                    if (!string.IsNullOrEmpty(newMechanic) && newMechanic != cell.Mechanic)
                    { cell.Serialized.FindProperty(PropMechanic).stringValue = newMechanic; touched = true; result.FieldsChanged++; }
                    if (newPages != null && !newPages.SequenceEqual(cell.Pages))
                    {
                        var p = cell.Serialized.FindProperty(PropPages);
                        p.arraySize = newPages.Count;
                        for (int i = 0; i < newPages.Count; i++) p.GetArrayElementAtIndex(i).stringValue = newPages[i];
                        touched = true; result.FieldsChanged++;
                    }
                    if (touched) { cell.Serialized.ApplyModifiedProperties(); result.DialogsChanged++; }
                    else result.DialogsUntouched++;
                }
            }
            if (result.DialogsChanged > 0) AssetDatabase.SaveAssets();
            RefreshIssues();
            return result;
        }

        // ──────────────────────────────────────────────────────────────────
        // Fingerprint (round-trip verification aid)
        // ──────────────────────────────────────────────────────────────────
        /// <summary>SHA-1 over every (id, lang, title, pages) in canonical order. Same texts → same hash.</summary>
        public string Fingerprint()
        {
            var sb = new StringBuilder();
            foreach (var row in Rows.OrderBy(r => r.Id, StringComparer.Ordinal))
                foreach (var lang in Languages)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var c) || c.Dialog == null) continue;
                    sb.Append(row.Id).Append('\u001f').Append(lang).Append('\u001f').Append(c.Title);
                    sb.Append('\u001f').Append(c.Mechanic);   // [TUT-TXT-1] every fingerprint changes ONCE at this step
                    foreach (var p in c.Pages) sb.Append('\u001f').Append(p);
                    sb.Append('\u001e');
                }
            using var sha = SHA1.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "").ToLowerInvariant();
        }

        public int DirtyCount()
        {
            int n = 0;
            foreach (var r in Rows) foreach (var c in r.ByLanguage.Values) if (c.Dialog != null && EditorUtility.IsDirty(c.Dialog)) n++;
            foreach (var c in Catalogs) if (EditorUtility.IsDirty(c.Catalog)) n++;
            return n;
        }
    }
}
#endif
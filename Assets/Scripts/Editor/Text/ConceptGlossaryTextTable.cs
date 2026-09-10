#if UNITY_EDITOR
// Place at: Assets/Scripts/Editor/Text/ConceptGlossaryTextTable.cs
// [TXT-2 / D-TAG-2=C · D-TAG-6] Editor model for the Concepts tab of GameTextWindow plus the
// registry index used by the Tutorial tab's tag checks.
//
//   ConceptRegistryEditorIndex — which concept ids exist in which registry (status keys from
//     every StatusEffectSO asset, SpecialKeywords enum names, glossary ids). "Known" = in any;
//     "duplicate" = in more than one. Runtime does not need this: ConceptTooltipResolver's
//     fixed order decides; the editor only makes a collision visible so it gets fixed.
//   ConceptGlossaryTextTable — one column per ConceptGlossarySO.languageCode, rows by id union,
//     reads/writes through SerializedObject (D1=A: the assets are the truth), CSV round-trip
//     with the same contract as the tutorial table (empty cell = no opinion, diff-only writes).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Status;
using ALWTTT.Tooltips;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    internal static class ConceptRegistryEditorIndex
    {
        public static readonly HashSet<string> StatusKeys = new(StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> KeywordNames = new(StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> GlossaryIds = new(StringComparer.OrdinalIgnoreCase);

        public static void Build()
        {
            StatusKeys.Clear(); KeywordNames.Clear(); GlossaryIds.Clear();
            foreach (var g in AssetDatabase.FindAssets($"t:{nameof(StatusEffectSO)}"))
            {
                var so = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(AssetDatabase.GUIDToAssetPath(g));
                if (so != null && !string.IsNullOrWhiteSpace(so.StatusKey)) StatusKeys.Add(so.StatusKey.Trim());
            }
            foreach (var name in Enum.GetNames(typeof(SpecialKeywords))) KeywordNames.Add(name);
            foreach (var g in AssetDatabase.FindAssets($"t:{nameof(ConceptGlossarySO)}"))
            {
                var gl = AssetDatabase.LoadAssetAtPath<ConceptGlossarySO>(AssetDatabase.GUIDToAssetPath(g));
                if (gl == null) continue;
                foreach (var e in gl.Entries)
                    if (e != null && !string.IsNullOrWhiteSpace(e.Id)) GlossaryIds.Add(e.Id.Trim());
            }
        }

        public static bool IsKnown(string id) =>
            StatusKeys.Contains(id) || KeywordNames.Contains(id) || GlossaryIds.Contains(id);

        /// <summary>Registries that claim this id, in resolver order. More than one = collision.</summary>
        public static List<string> Owners(string id)
        {
            var l = new List<string>(3);
            if (StatusKeys.Contains(id)) l.Add("status");
            if (KeywordNames.Contains(id)) l.Add("keyword");
            if (GlossaryIds.Contains(id)) l.Add("glossary");
            return l;
        }
    }

    internal sealed class ConceptGlossaryTextTable
    {
        private const string PropLanguage = "languageCode";
        private const string PropEntries = "entries";
        private const string PropId = "id";
        private const string PropName = "displayName";
        private const string PropDesc = "description";

        public const string FieldName = "displayName";
        public const string FieldDesc = "description";

        private static readonly Regex IdRx = new(@"^[a-z0-9_]+$", RegexOptions.Compiled);

        public sealed class GlossaryInfo
        {
            public ConceptGlossarySO Glossary;
            public SerializedObject Serialized;
            public string Path;
            public string Language;
            public bool DuplicateLanguage;
        }

        public sealed class Cell
        {
            public GlossaryInfo Owner;
            public int Index;                       // element index inside Owner.entries
            public SerializedProperty Element => Owner.Serialized.FindProperty(PropEntries).GetArrayElementAtIndex(Index);
            public string Name => Element.FindPropertyRelative(PropName).stringValue ?? "";
            public string Description => Element.FindPropertyRelative(PropDesc).stringValue ?? "";
        }

        public sealed class Row
        {
            public string Id;
            public readonly Dictionary<string, Cell> ByLanguage = new();
            public readonly List<string> Issues = new();
            public bool HasIssues => Issues.Count > 0;
        }

        public readonly List<GlossaryInfo> Glossaries = new();
        public readonly List<string> Languages = new();
        public readonly List<Row> Rows = new();

        public void Rebuild()
        {
            Glossaries.Clear(); Languages.Clear(); Rows.Clear();
            ConceptRegistryEditorIndex.Build();

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(ConceptGlossarySO)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var gl = AssetDatabase.LoadAssetAtPath<ConceptGlossarySO>(path);
                if (gl == null) continue;
                var so = new SerializedObject(gl);
                Glossaries.Add(new GlossaryInfo
                {
                    Glossary = gl,
                    Serialized = so,
                    Path = path,
                    Language = TutorialTextTable.NormaliseLanguage(so.FindProperty(PropLanguage).stringValue),
                });
            }
            Glossaries.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));

            var claimed = new HashSet<string>();
            foreach (var g in Glossaries)
            {
                if (string.IsNullOrEmpty(g.Language)) continue;
                if (!claimed.Add(g.Language)) { g.DuplicateLanguage = true; continue; }
                Languages.Add(g.Language);
            }
            Languages.Sort((a, b) => a == "en" ? -1 : b == "en" ? 1 : string.CompareOrdinal(a, b));

            var byId = new Dictionary<string, Row>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in Glossaries)
            {
                if (string.IsNullOrEmpty(g.Language) || g.DuplicateLanguage) continue;
                var arr = g.Serialized.FindProperty(PropEntries);
                for (int i = 0; i < arr.arraySize; i++)
                {
                    string id = (arr.GetArrayElementAtIndex(i).FindPropertyRelative(PropId).stringValue ?? "").Trim();
                    if (string.IsNullOrEmpty(id)) id = $"<empty #{i}>";
                    if (!byId.TryGetValue(id, out var row)) byId[id] = row = new Row { Id = id };
                    if (row.ByLanguage.ContainsKey(g.Language)) { row.Issues.Add($"DUPLICATE id in {g.Language}"); continue; }
                    row.ByLanguage[g.Language] = new Cell { Owner = g, Index = i };
                }
            }
            Rows.AddRange(byId.Values);
            Rows.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            RefreshIssues();
        }

        public void UpdateSerialized() { foreach (var g in Glossaries) g.Serialized.Update(); }

        public void RefreshIssues()
        {
            foreach (var row in Rows)
            {
                row.Issues.RemoveAll(s => !s.StartsWith("DUPLICATE id", StringComparison.Ordinal));
                if (!IdRx.IsMatch(row.Id)) row.Issues.Add("ID invalid (use [a-z0-9_])");
                // [D-TAG-2=C] one home per concept: an id that a status or a keyword already owns must
                // not ALSO live here — the resolver would never reach it, and the text would rot.
                if (ConceptRegistryEditorIndex.StatusKeys.Contains(row.Id)) row.Issues.Add("DUPLICATE: status owns it");
                if (ConceptRegistryEditorIndex.KeywordNames.Contains(row.Id)) row.Issues.Add("DUPLICATE: keyword owns it");
                foreach (var lang in Languages)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var c)) { row.Issues.Add($"MISSING {lang}"); continue; }
                    if (string.IsNullOrWhiteSpace(c.Name)) row.Issues.Add($"NO NAME {lang}");
                    if (string.IsNullOrWhiteSpace(c.Description)) row.Issues.Add($"NO DESCRIPTION {lang}");
                    if (ConceptTagRenderer.HasUnbalancedLinks(c.Description)) row.Issues.Add($"TAG unclosed {lang}");
                }
            }
        }

        public void SetGlossaryLanguage(GlossaryInfo g, string code)
        {
            g.Serialized.Update();
            g.Serialized.FindProperty(PropLanguage).stringValue = TutorialTextTable.NormaliseLanguage(code);
            g.Serialized.ApplyModifiedProperties();
        }

        /// <summary>Appends an entry with this id to the glossary of <paramref name="lang"/>. No file is
        /// created (entries are list elements), so this is cheap and safe to do from CSV import too.</summary>
        public bool CreateEntry(string id, string lang, out string error)
        {
            error = null;
            id = (id ?? "").Trim();
            if (!IdRx.IsMatch(id)) { error = $"'{id}' is not a valid id ([a-z0-9_])."; return false; }
            var g = Glossaries.FirstOrDefault(x => x.Language == lang && !x.DuplicateLanguage);
            if (g == null) { error = $"No ConceptGlossarySO with languageCode '{lang}'."; return false; }
            g.Serialized.Update();
            var arr = g.Serialized.FindProperty(PropEntries);
            for (int i = 0; i < arr.arraySize; i++)
                if (string.Equals(arr.GetArrayElementAtIndex(i).FindPropertyRelative(PropId).stringValue?.Trim(), id, StringComparison.OrdinalIgnoreCase))
                { error = $"'{id}' already exists in '{g.Glossary.name}'."; return false; }
            arr.arraySize++;
            var el = arr.GetArrayElementAtIndex(arr.arraySize - 1);
            el.FindPropertyRelative(PropId).stringValue = id;
            el.FindPropertyRelative(PropName).stringValue = "";
            el.FindPropertyRelative(PropDesc).stringValue = "";
            g.Serialized.ApplyModifiedProperties();
            return true;
        }

        // ── CSV ─────────────────────────────────────────────────────────────
        public GameTextCsv.Table ToCsvTable()
        {
            var t = new GameTextCsv.Table();
            t.Header.Add(GameTextCsv.ColId); t.Header.Add(GameTextCsv.ColField); t.Header.AddRange(Languages);
            foreach (var row in Rows)
            {
                t.Rows.Add(Record(row, FieldName, l => row.ByLanguage.TryGetValue(l, out var c) ? c.Name : ""));
                t.Rows.Add(Record(row, FieldDesc, l => row.ByLanguage.TryGetValue(l, out var c) ? c.Description : ""));
            }
            return t;
        }

        private string[] Record(Row row, string field, Func<string, string> cellFor)
        {
            var rec = new string[2 + Languages.Count];
            rec[0] = row.Id; rec[1] = field;
            for (int i = 0; i < Languages.Count; i++) rec[2 + i] = cellFor(Languages[i]) ?? "";
            return rec;
        }

        /// <summary>Same contract as the tutorial import (diff-only writes, empty cell = no opinion,
        /// unknown language column aborts) with ONE difference: an id absent from a glossary is
        /// CREATED there, because a glossary entry is a list element, not an asset file.</summary>
        public TutorialTextTable.ImportResult ApplyCsv(GameTextCsv.Table table)
        {
            var result = new TutorialTextTable.ImportResult();
            if (table.Header.Count < 2 ||
                !string.Equals(table.Header[0].Trim(), GameTextCsv.ColId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(table.Header[1].Trim(), GameTextCsv.ColField, StringComparison.OrdinalIgnoreCase))
            { result.Errors.Add($"Header must start with '{GameTextCsv.ColId}', '{GameTextCsv.ColField}'."); return result; }

            var langCols = new Dictionary<string, int>();
            for (int i = 2; i < table.Header.Count; i++)
            {
                string code = TutorialTextTable.NormaliseLanguage(table.Header[i]);
                if (string.IsNullOrEmpty(code)) continue;
                if (!Languages.Contains(code)) { result.Errors.Add($"Unknown language column '{table.Header[i]}'. Known: {string.Join(", ", Languages)}."); continue; }
                langCols[code] = i;
            }
            if (result.Aborted) return result;

            // id → field → lang → value
            var pending = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);
            for (int r = 0; r < table.Rows.Count; r++)
            {
                var rec = table.Rows[r];
                if (rec.Length < 2) continue;
                string id = rec[0].Trim(), field = rec[1].Trim();
                if (string.IsNullOrEmpty(id)) continue;
                if (field != FieldName && field != FieldDesc) { result.Skipped.Add($"row {r + 2}: unknown field '{field}'"); continue; }
                if (!pending.TryGetValue(id, out var byField)) pending[id] = byField = new();
                if (!byField.TryGetValue(field, out var byLang)) byField[field] = byLang = new();
                foreach (var kv in langCols) byLang[kv.Key] = kv.Value < rec.Length ? rec[kv.Value] ?? "" : "";
            }

            bool anyCreated = false;
            foreach (var kvId in pending)
            {
                string id = kvId.Key;
                if (!IdRx.IsMatch(id)) { result.Skipped.Add($"'{id}': invalid id ([a-z0-9_])"); continue; }
                foreach (var lang in langCols.Keys)
                {
                    string newName = kvId.Value.TryGetValue(FieldName, out var nl) && nl.TryGetValue(lang, out var nv) ? nv : null;
                    string newDesc = kvId.Value.TryGetValue(FieldDesc, out var dl) && dl.TryGetValue(lang, out var dv) ? dv : null;
                    bool hasOpinion = !string.IsNullOrEmpty(newName) || !string.IsNullOrEmpty(newDesc);
                    if (!hasOpinion) { result.DialogsUntouched++; continue; }

                    var row = Rows.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                    if (row == null || !row.ByLanguage.ContainsKey(lang))
                    {
                        if (!CreateEntry(id, lang, out string err)) { result.Skipped.Add($"'{id}' [{lang}]: {err}"); continue; }
                        anyCreated = true;
                        Rebuild();   // cheap; picks up the new element index
                        row = Rows.First(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                    }
                    var cell = row.ByLanguage[lang];
                    cell.Owner.Serialized.Update();
                    bool touched = false;
                    if (!string.IsNullOrEmpty(newName) && newName != cell.Name)
                    { cell.Element.FindPropertyRelative(PropName).stringValue = newName; touched = true; result.FieldsChanged++; }
                    if (!string.IsNullOrEmpty(newDesc) && newDesc != cell.Description)
                    { cell.Element.FindPropertyRelative(PropDesc).stringValue = newDesc; touched = true; result.FieldsChanged++; }
                    if (touched) { cell.Owner.Serialized.ApplyModifiedProperties(); result.DialogsChanged++; }
                    else result.DialogsUntouched++;
                }
            }
            if (result.DialogsChanged > 0 || anyCreated) AssetDatabase.SaveAssets();
            Rebuild();
            return result;
        }

        public string Fingerprint()
        {
            var sb = new StringBuilder();
            foreach (var row in Rows.OrderBy(r => r.Id, StringComparer.Ordinal))
                foreach (var lang in Languages)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var c)) continue;
                    sb.Append(row.Id).Append('\u001f').Append(lang).Append('\u001f').Append(c.Name).Append('\u001f').Append(c.Description).Append('\u001e');
                }
            using var sha = SHA1.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "").ToLowerInvariant();
        }

        public int DirtyCount() => Glossaries.Count(g => EditorUtility.IsDirty(g.Glossary));
    }
}
#endif
#if UNITY_EDITOR
// Place at: Assets/Scripts/Editor/Text/GlossarySectionTable.cs
// [TXT-4 / D-TXT4-3=b′ · D-TXT4-4] Editor model for ONE single-text-field list inside
// ConceptGlossarySO — used twice: `cardNames` (Cards tab, field displayName, ids = CardDefinition.Id)
// and `templates` (Templates tab, field text, ids = GameTextTemplateIds.All). Same contract as
// ConceptGlossaryTextTable: one column per glossary languageCode, rows by id union, reads/writes
// through SerializedObject (D1=A: the assets are the truth), CSV round-trip with the tutorial
// codec (empty cell = no opinion, diff-only writes, unknown language column aborts), SHA-1
// fingerprint, and a COVERAGE block for ids the section MUST contain (every CardDefinition id;
// every declared template id) — the TXT-3 rule, applied to two more populations.
// Extra badges over the concept table (GameTextRichText): PLACEHOLDERS differ, SPRITES differ,
// SPRITE index, SPRITE unknown, TAG unsupported.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ALWTTT.Data;
using ALWTTT.Tooltips;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    internal sealed class GlossarySectionTable
    {
        public sealed class Spec
        {
            public string Label;                       // "Cards" / "Templates"
            public string ListProp;                    // serialized list name on ConceptGlossarySO
            public string IdProp;                      // "id"
            public string TextProp;                    // "displayName" / "text"
            public string CsvField;                    // CSV `field` value
            public Regex IdRx;                         // id validator
            public string IdRule;                      // human text for the badge
            public Func<IEnumerable<string>> RequiredIds;   // coverage set
            public string RequiredKind;                // "card" / "template" (coverage line prefix)
            public bool CheckPlaceholders;             // templates: {n} parity + arg count
        }

        public static readonly Spec CardsSpec = new()
        {
            Label = "Cards",
            ListProp = "cardNames",
            IdProp = "id",
            TextProp = "displayName",
            CsvField = "displayName",
            IdRx = new Regex(@"^[A-Za-z0-9_\-]+$", RegexOptions.Compiled),
            IdRule = "[A-Za-z0-9_-]",
            RequiredIds = () => AssetDatabase.FindAssets($"t:{nameof(ALWTTT.Cards.CardDefinition)}")
                .Select(g => AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.Id)).Select(c => c.Id.Trim()),
            RequiredKind = "card",
            CheckPlaceholders = false,
        };

        public static readonly Spec TemplatesSpec = new()
        {
            Label = "Templates",
            ListProp = "templates",
            IdProp = "id",
            TextProp = "text",
            CsvField = "text",
            IdRx = new Regex(@"^[a-z0-9_]+$", RegexOptions.Compiled),
            IdRule = "[a-z0-9_]",
            RequiredIds = () => GameTextTemplateIds.All,
            RequiredKind = "template",
            CheckPlaceholders = true,
        };

        private const string PropLanguage = "languageCode";

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
            public int Index;
            public SerializedProperty Element => Owner.Serialized.FindProperty(Table.S.ListProp).GetArrayElementAtIndex(Index);
            public GlossarySectionTable Table;
            public string Text => Element.FindPropertyRelative(Table.S.TextProp).stringValue ?? "";
        }

        public sealed class Row
        {
            public string Id;
            public readonly Dictionary<string, Cell> ByLanguage = new();
            public readonly List<string> Issues = new();
            public bool HasIssues => Issues.Count > 0;
        }

        public readonly Spec S;
        public readonly List<GlossaryInfo> Glossaries = new();
        public readonly List<string> Languages = new();
        public readonly List<Row> Rows = new();

        public GlossarySectionTable(Spec spec) { S = spec; }

        public void Rebuild()
        {
            Glossaries.Clear(); Languages.Clear(); Rows.Clear();
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
                var arr = g.Serialized.FindProperty(S.ListProp);
                if (arr == null) continue;   // glossary asset predates TXT-4 (script not yet updated)
                for (int i = 0; i < arr.arraySize; i++)
                {
                    string id = (arr.GetArrayElementAtIndex(i).FindPropertyRelative(S.IdProp).stringValue ?? "").Trim();
                    if (string.IsNullOrEmpty(id)) id = $"<empty #{i}>";
                    if (!byId.TryGetValue(id, out var row)) byId[id] = row = new Row { Id = id };
                    if (row.ByLanguage.ContainsKey(g.Language)) { row.Issues.Add($"DUPLICATE id in {g.Language}"); continue; }
                    row.ByLanguage[g.Language] = new Cell { Owner = g, Index = i, Table = this };
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
                if (!S.IdRx.IsMatch(row.Id)) row.Issues.Add($"ID invalid (use {S.IdRule})");

                HashSet<int> refPlaceholders = null; string refPlaceholderLang = null;
                HashSet<string> refSprites = null; string refSpriteLang = null;
                foreach (var lang in Languages)
                {
                    if (!row.ByLanguage.TryGetValue(lang, out var c)) { row.Issues.Add($"MISSING {lang}"); continue; }
                    string text = c.Text;
                    if (string.IsNullOrWhiteSpace(text)) { row.Issues.Add($"NO TEXT {lang}"); continue; }

                    if (ALWTTT.Tooltips.ConceptTagRenderer.HasUnbalancedLinks(text)) row.Issues.Add($"TAG unclosed {lang}");
                    var bad = GameTextRichText.UnsupportedTags(text);
                    if (bad.Count > 0) row.Issues.Add($"TAG unsupported {lang}: {string.Join(", ", bad)}");
                    if (GameTextRichText.UsesSpriteIndex(text)) row.Issues.Add($"SPRITE index {lang} (use name=)");

                    var sprites = GameTextRichText.SpriteNames(text);
                    if (GameTextRichText.SpriteAssetAvailable)
                        foreach (var sName in sprites)
                            if (!GameTextRichText.SpriteExists(sName)) row.Issues.Add($"SPRITE unknown {lang}: {sName}");
                    if (refSprites == null) { refSprites = sprites; refSpriteLang = lang; }
                    else if (!GameTextRichText.SetsEqual(refSprites, sprites)) row.Issues.Add($"SPRITES differ {refSpriteLang}/{lang}");

                    if (S.CheckPlaceholders)
                    {
                        var ph = GameTextRichText.Placeholders(text);
                        if (refPlaceholders == null) { refPlaceholders = ph; refPlaceholderLang = lang; }
                        else if (!GameTextRichText.SetsEqual(refPlaceholders, ph)) row.Issues.Add($"PLACEHOLDERS differ {refPlaceholderLang}/{lang}");
                    }
                }
            }
        }

        /// <summary>Ids the section MUST contain (non-empty text in every language) but does not.</summary>
        public List<string> MissingCoverage()
        {
            var lines = new List<string>();
            foreach (var id in S.RequiredIds().Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.Ordinal))
            {
                var row = Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
                var missing = new List<string>();
                foreach (var lang in Languages)
                    if (row == null || !row.ByLanguage.TryGetValue(lang, out var c) || string.IsNullOrWhiteSpace(c.Text))
                        missing.Add(lang);
                if (missing.Count > 0) lines.Add($"{S.RequiredKind} '{id}' — missing: {string.Join(", ", missing)}");
            }
            return lines;
        }

        /// <summary>Ids present in the section that no producer declares (templates) / no CardDefinition owns (cards).
        /// Informational: an orphan is not an error, but it is dead text.</summary>
        public List<string> Orphans()
        {
            var required = new HashSet<string>(S.RequiredIds(), StringComparer.OrdinalIgnoreCase);
            return Rows.Where(r => !required.Contains(r.Id)).Select(r => r.Id).ToList();
        }

        public bool CreateEntry(string id, string lang, out string error)
        {
            error = null;
            id = (id ?? "").Trim();
            if (!S.IdRx.IsMatch(id)) { error = $"'{id}' is not a valid id ({S.IdRule})."; return false; }
            var g = Glossaries.FirstOrDefault(x => x.Language == lang && !x.DuplicateLanguage);
            if (g == null) { error = $"No ConceptGlossarySO with languageCode '{lang}'."; return false; }
            g.Serialized.Update();
            var arr = g.Serialized.FindProperty(S.ListProp);
            if (arr == null) { error = $"'{g.Glossary.name}' has no '{S.ListProp}' list — update ConceptGlossarySO.cs first."; return false; }
            for (int i = 0; i < arr.arraySize; i++)
                if (string.Equals(arr.GetArrayElementAtIndex(i).FindPropertyRelative(S.IdProp).stringValue?.Trim(), id, StringComparison.OrdinalIgnoreCase))
                { error = $"'{id}' already exists in '{g.Glossary.name}'."; return false; }
            arr.arraySize++;
            var el = arr.GetArrayElementAtIndex(arr.arraySize - 1);
            el.FindPropertyRelative(S.IdProp).stringValue = id;
            el.FindPropertyRelative(S.TextProp).stringValue = "";
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
                var rec = new string[2 + Languages.Count];
                rec[0] = row.Id; rec[1] = S.CsvField;
                for (int i = 0; i < Languages.Count; i++)
                    rec[2 + i] = row.ByLanguage.TryGetValue(Languages[i], out var c) ? c.Text : "";
                t.Rows.Add(rec);
            }
            return t;
        }

        /// <summary>Same contract as the concept import: diff-only, empty cell = no opinion, unknown
        /// language column aborts, an id absent from a glossary is CREATED there.</summary>
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

            var pending = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);   // id → lang → value
            for (int r = 0; r < table.Rows.Count; r++)
            {
                var rec = table.Rows[r];
                if (rec.Length < 2) continue;
                string id = rec[0].Trim(), field = rec[1].Trim();
                if (string.IsNullOrEmpty(id)) continue;
                if (!string.Equals(field, S.CsvField, StringComparison.Ordinal)) { result.Skipped.Add($"row {r + 2}: unknown field '{field}' (expected '{S.CsvField}')"); continue; }
                if (!pending.TryGetValue(id, out var byLang)) pending[id] = byLang = new();
                foreach (var kv in langCols) byLang[kv.Key] = kv.Value < rec.Length ? rec[kv.Value] ?? "" : "";
            }

            bool anyCreated = false;
            foreach (var kvId in pending)
            {
                string id = kvId.Key;
                if (!S.IdRx.IsMatch(id)) { result.Skipped.Add($"'{id}': invalid id ({S.IdRule})"); continue; }
                foreach (var lang in langCols.Keys)
                {
                    string newText = kvId.Value.TryGetValue(lang, out var v) ? v : null;
                    if (string.IsNullOrEmpty(newText)) { result.DialogsUntouched++; continue; }

                    var row = Rows.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                    if (row == null || !row.ByLanguage.ContainsKey(lang))
                    {
                        if (!CreateEntry(id, lang, out string err)) { result.Skipped.Add($"'{id}' [{lang}]: {err}"); continue; }
                        anyCreated = true;
                        Rebuild();
                        row = Rows.First(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                    }
                    var cell = row.ByLanguage[lang];
                    cell.Owner.Serialized.Update();
                    if (newText != cell.Text)
                    {
                        cell.Element.FindPropertyRelative(S.TextProp).stringValue = newText;
                        cell.Owner.Serialized.ApplyModifiedProperties();
                        result.FieldsChanged++; result.DialogsChanged++;
                    }
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
                    sb.Append(row.Id).Append('\u001f').Append(lang).Append('\u001f').Append(c.Text).Append('\u001e');
                }
            using var sha = SHA1.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "").ToLowerInvariant();
        }

        public int DirtyCount() => Glossaries.Count(g => EditorUtility.IsDirty(g.Glossary));
    }
}
#endif
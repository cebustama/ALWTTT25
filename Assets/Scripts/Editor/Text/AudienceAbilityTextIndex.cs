#if UNITY_EDITOR
// Place at: Assets/Scripts/Editor/Text/AudienceAbilityTextIndex.cs
// [TEXT-5 / D-T5-1=A · D-T5-1a=A] The sixth text population (SSoT_Game_Text §1, F-TXT4-7) is
// authored as DATA in AudienceCharacterData assets, not as code. Its coverage set therefore
// cannot be a list of constants (the way GameTextTemplateIds.All is for templates): it has to
// be enumerated from the assets. This file does two things and nothing else:
//
//   AbilityIds()  — every AudienceAbilityData.abilityId in every AudienceCharacterData asset,
//                   read through SerializedObject. An EMPTY id is NOT skipped: it is emitted as
//                   the sentinel "EMPTY:<asset>[<index>]", an id no glossary can contain, so it
//                   surfaces as a COVERAGE error that names the asset instead of vanishing —
//                   the same hole GameTextTemplateIds.All leaves for an undeclared template,
//                   closed here by construction.
//   Seed menu     — ALWTTT ▸ Text ▸ Seed Ability Ids. Idempotent: only EMPTY ids are written,
//                   as "ab_<slug of abilityName>", de-duplicated across the whole roster.
//                   Prints the census (id, abilityName, intention.contentText) as Concepts-CSV
//                   rows to the Console AND copies it to the clipboard, so the EN column of the
//                   glossary import is the current data verbatim (EN behaviour unchanged).
//
// No public setters on the SO are used (TXT-1 invariant): writes go through SerializedObject.
// Runtime never touches this file; the glossary entry keyed by abilityId is what the audience
// canvas reads (GameText.ConceptName / ConceptDescription).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ALWTTT.Data;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring
{
    internal static class AudienceAbilityTextIndex
    {
        private const string PropAbilityList = "abilityList";
        private const string PropAbilityId = "abilityId";
        private const string PropAbilityName = "abilityName";
        private const string PropIntention = "intention";
        private const string IdPrefix = "ab_";
        private const string EmptySentinelPrefix = "EMPTY:";

        private static readonly Regex NonIdChars = new(@"[^a-z0-9]+", RegexOptions.Compiled);

        /// <summary>One record per authored ability, in asset-name order then list order.</summary>
        internal sealed class Record
        {
            public AudienceCharacterData Asset;
            public int Index;
            public string Id;            // trimmed; "" when unset
            public string AbilityName;   // developer label (seed source only)
            public string IntentionText; // AudienceIntentionData.ContentText, developer label since TEXT-5
            public bool IsEmpty => string.IsNullOrWhiteSpace(Id);
            public string CoverageId => IsEmpty ? $"{EmptySentinelPrefix}{Asset.name}[{Index}]" : Id;
        }

        /// <summary>Coverage set for the Concepts tab: every abilityId, EMPTY ones as sentinels.</summary>
        public static IEnumerable<string> AbilityIds() => Scan().Select(r => r.CoverageId);

        /// <summary>True when this coverage id is an EMPTY sentinel (for badge wording only).</summary>
        public static bool IsEmptySentinel(string id) =>
            !string.IsNullOrEmpty(id) && id.StartsWith(EmptySentinelPrefix, StringComparison.Ordinal);

        internal static List<Record> Scan()
        {
            var list = new List<Record>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AudienceCharacterData)}"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null) continue;

                using var so = new SerializedObject(asset);
                var arr = so.FindProperty(PropAbilityList);
                if (arr == null || !arr.isArray) continue;

                for (int i = 0; i < arr.arraySize; i++)
                {
                    var el = arr.GetArrayElementAtIndex(i);
                    var pId = el.FindPropertyRelative(PropAbilityId);
                    var pName = el.FindPropertyRelative(PropAbilityName);
                    var pIntent = el.FindPropertyRelative(PropIntention);
                    var intent = pIntent != null ? pIntent.objectReferenceValue as AudienceIntentionData : null;

                    list.Add(new Record
                    {
                        Asset = asset,
                        Index = i,
                        Id = pId != null ? (pId.stringValue ?? "").Trim() : "",
                        AbilityName = pName != null ? (pName.stringValue ?? "").Trim() : "",
                        IntentionText = intent != null ? (intent.ContentText ?? "").Trim() : "",
                    });
                }
            }
            return list.OrderBy(r => r.Asset.name, StringComparer.Ordinal).ThenBy(r => r.Index).ToList();
        }

        // ── Seeder ───────────────────────────────────────────────────────────

        [MenuItem("ALWTTT/Text/Seed Ability Ids")]
        private static void SeedAbilityIds()
        {
            var records = Scan();
            var taken = new HashSet<string>(records.Where(r => !r.IsEmpty).Select(r => r.Id), StringComparer.OrdinalIgnoreCase);

            int written = 0, skipped = 0;
            var touched = new HashSet<AudienceCharacterData>();

            foreach (var group in records.GroupBy(r => r.Asset))
            {
                using var so = new SerializedObject(group.Key);
                var arr = so.FindProperty(PropAbilityList);
                bool dirty = false;

                foreach (var r in group)
                {
                    if (!r.IsEmpty) { skipped++; continue; }

                    string baseId = IdPrefix + Slug(r.AbilityName);
                    if (baseId == IdPrefix) baseId = IdPrefix + "ability_" + r.Index;   // unnamed ability

                    string id = baseId;
                    for (int n = 2; taken.Contains(id); n++) id = $"{baseId}_{n}";
                    taken.Add(id);

                    arr.GetArrayElementAtIndex(r.Index).FindPropertyRelative(PropAbilityId).stringValue = id;
                    r.Id = id;
                    dirty = true;
                    written++;
                }

                if (dirty)
                {
                    so.ApplyModifiedProperties();   // undoable; the asset is the truth (D1=A)
                    touched.Add(group.Key);
                }
            }

            if (touched.Count > 0) AssetDatabase.SaveAssets();

            // Census as Concepts-CSV rows: header + displayName + description per ability.
            // EN = current data verbatim. ES left empty ("no opinion" cell, filled by the translator).
            var csv = new StringBuilder();
            csv.AppendLine("id,field,en,es");
            foreach (var r in records)
            {
                csv.AppendLine($"{r.Id},displayName,{Csv(r.AbilityName)},");
                csv.AppendLine($"{r.Id},description,{Csv(r.IntentionText)},");
            }
            EditorGUIUtility.systemCopyBuffer = csv.ToString();

            var dupes = records.GroupBy(r => r.Id, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            Debug.Log($"[TEXT-5] Seed Ability Ids — {records.Count} abilities in {records.Select(r => r.Asset).Distinct().Count()} assets · " +
                      $"{written} id(s) written · {skipped} already set · assets saved: {touched.Count}" +
                      (dupes.Count > 0 ? $" · SHARED ids (one glossary entry serves several abilities — intended under D-T5-5=A): {string.Join(", ", dupes)}" : "") +
                      "\nCensus (Concepts CSV, EN verbatim) copied to the clipboard:\n" + csv);
        }

        private static string Slug(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim().ToLowerInvariant();
            t = RemoveDiacritics(t);
            t = NonIdChars.Replace(t, "_").Trim('_');
            return t;
        }

        private static string RemoveDiacritics(string s)
        {
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (char c in norm)
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>RFC-4180 cell: quoted when it contains a comma, quote or newline.</summary>
        private static string Csv(string s)
        {
            s ??= "";
            return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }
}
#endif